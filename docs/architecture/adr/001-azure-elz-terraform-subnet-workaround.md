# Decision - 0001 - Azure ELZ Terraform Subnet Workaround

## Context and Problem Statement

When deploying to the Enterprise Landing Zone (ELZ) we experienced a major blocker. There was a policy that kept failing our terraform infrastructure deployment. There was a lot of trial and error from research and colleague sugeestion.

We found that:

- The subnet resource was being rejected due to the following policy:

![Screenshot 2025-10-22 at 17.32.50.png](../../images/subnet-policy.png)

- To resolve this, it was suggested to use the following to the property (but in the current teraform via it has been deprecated):
```terraform
resource "azurerm_subnet" "gettoananswer-subnet" {
  ...
  network_security_group_id = azurerm_network_security_group.gettoananswer-nsg.id
  ...
}
```

- The other suggestion was to use association, but this too was unsuccessful:

```terraform
resource "azurerm_subnet_network_security_group_association" "default" {
  subnet_id                 = azurerm_subnet.gettoananswer_main_subnet.id
  network_security_group_id = azurerm_network_security_group.gettoananswer-nsg.id
}
```

As part of further investigations, we noted that:

- We found that the issue we ELZ specific and a bug was raised to fix this, so an exemption or workaround was in order.

## Considered Options

- Use the azapi provider to customise the creation of the subnet.
- Doing clickops to create with the association then import the resource to complete provisioning.
- Using a custom provider to do the creation based on an open PR fix [here](https://github.com/hashicorp/terraform-provider-azurerm/pull/28985).
- Adding an exception to the policy.
- Get ELZ admins to Ccange the policy effect type from `deny` to `audit`.

### Evaluation

|    Criteria     | Comment                                                                     | Azapi  | Clickops | Custom | exemption | Alter Policy | 
|:---------------:|:----------------------------------------------------------------------------|:------:|:--------:|:------:|:---------:|:------------:|
|   Automatable   | Can be implemented into an IaC script, automated in different environments. |   5    |    0     |   5    |     1     |      1       | 
|   Repeatable    | Can other teams leverage this solution/workaround for their use cases too.  |   5    |    2     |   4    |     3     |      3       | 
|     Effort      | How easy is it to setup/configure and deploy.                               |   5    |    3     |   3    |     1     |      1       |
| Error Resistant | Can human error be avoided/mitigated via this approach.                     |   5    |    1     |   4    |     4     |      4       |
|    **Total**    |                                                                             | **20** |  **7**   | **16** |   **9**   |    **9**     |

## Decision Outcome

Based on the analysis above, we have chose to switch to Azapi as this required the least amount of effort, waas easily repeatable by other teams and did require an admin support.

With the azapi resource, we are able to create the subnet and then associate it with a network security group.

This is the replacement (azapi) resource:

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