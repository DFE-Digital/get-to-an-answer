# Decision - 0001 - Azure ELZ Terraform Subnet Workaround

## Context and Problem Statement

When deploying to the Enterprise Landing Zone (ELZ) we encountered a major blocker: a policy repeatedly caused our Terraform infrastructure deployment to fail. There was considerable trial and error based on research and colleagues’ suggestions.

We found that:

- The subnet resource was being rejected due to the following policy:

![Screenshot 2025-10-22 at 17.32.50.png](../../images/subnet-policy.png)

- To resolve this, it was suggested to use the property below (however, in the current Terraform provider it has been deprecated):
```terraform
resource "azurerm_subnet" "gettoananswer-subnet" {
  ...
  network_security_group_id = azurerm_network_security_group.gettoananswer-nsg.id
  ...
}
```

- Another suggestion was to use association resource, but this was also unsuccessful:

```terraform
resource "azurerm_subnet_network_security_group_association" "default" {
  subnet_id                 = azurerm_subnet.gettoananswer_main_subnet.id
  network_security_group_id = azurerm_network_security_group.gettoananswer-nsg.id
}
```

As part of further investigations, we noted that:

- The issue was ELZ-specific and a bug was raised to fix it, so an exemption or workaround was required.

## Considered Options

- Use the AzAPI provider to customise the creation of the subnet.
- Use click-ops to create the subnet with the association, then import the resource to complete provisioning.
- Use a custom provider to implement the creation based on the open PR fix [here](https://github.com/hashicorp/terraform-provider-azurerm/pull/28985).
- Add an exception to the policy.
- Ask ELZ admins to change the policy effect from `deny` to `audit`.

### Evaluation

|    Criteria     | Comment                                                                     | AzAPI | Click-ops | Custom | Exemption | Alter Policy |
|:---------------:|:----------------------------------------------------------------------------|:-----:|:---------:|:------:|:---------:|:------------:|
|   Automatable   | Can be implemented in IaC and automated across environments.                |   5   |     0     |   5    |     1     |      1       |
|   Repeatable    | Can other teams leverage this solution/workaround for their use cases too?  |   5   |     2     |   4    |     3     |      3       |
|     Effort      | How easy is it to set up/configure and deploy?                              |   5   |     3     |   3    |     1     |      1       |
| Error Resistant | Can human error be avoided/mitigated via this approach?                     |   5   |     1     |   4    |     4     |      4       |
|    **Total**    |                                                                             | **20**|  **7**    | **16** |  **9**    |   **9**      |

## Decision Outcome

Based on the analysis above, we chose to switch to AzAPI, as it required the least effort, was easily repeatable by other teams, and did not require admin support.

With the AzAPI resource we can create the subnet and then associate it with a network security group.

This is the replacement (AzAPI) resource:


```terraform
resource "azapi_resource" "gettoananswer_main_subnet" {
  type      = "Microsoft.Network/virtualNetworks/subnets@2024-05-01"
  name      = "${var.prefix}subnet-uks-gtaa"
  parent_id = azurerm_virtual_network.gettoananswer_vnet.id

  body =  {
    properties = {
      addressPrefixes = [...]
      delegations = [{
        name = "asp-delegation"
        properties = {
          serviceName = "Microsoft.Web/serverFarms"
        }
      }]
      serviceEndpoints = [
        {
          service   = "Microsoft.Sql"
          locations = [azurerm_resource_group.gettoananswer-rg.location]
        }
      ]
      # the association with the network security group
      networkSecurityGroup = {
        id = azurerm_network_security_group.gettoananswer-nsg.id
      }
    }
  }

  depends_on = [azurerm_network_security_group.gettoananswer-nsg]
}
```

### Considerations on selected technology

Due to the flexibility of the `azapi_resource`, you'll have to periodically check when changes are made to the resource attributes and change according, because `terraform validation` doesn't flag this.