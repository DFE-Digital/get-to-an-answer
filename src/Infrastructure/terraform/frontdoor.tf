resource "azurerm_cdn_frontdoor_profile" "frontdoor-web-profile" {
  name                = "${var.prefix}fwb-uks-web"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  sku_name            = "${var.azure_frontdoor_scale}_AzureFrontDoor"
  tags = {
    Environment = var.env
    Product     = var.product
  }
}

// One shared origin group (simple single-origin per route usage)
resource "azurerm_cdn_frontdoor_origin_group" "frontdoor-origin-group" {
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.id
  name                     = "${var.prefix}fog-uks-web"
  session_affinity_enabled = false

  health_probe {
    interval_in_seconds = 60
    protocol            = "Https"
    request_type        = "GET"
    path                = "/health"
  }

  load_balancing {
    sample_size                 = 4
    successful_samples_required = 2
  }
}

// Origins
resource "azurerm_cdn_frontdoor_origin" "frontdoor-api-origin" {
  cdn_frontdoor_origin_group_id  = azurerm_cdn_frontdoor_origin_group.frontdoor-origin-group.id
  certificate_name_check_enabled = false
  host_name                      = azurerm_linux_web_app.gettoananswer-api.default_hostname
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = azurerm_linux_web_app.gettoananswer-api.default_hostname
  priority                       = 1
  weight                         = 1
  name                           = "${var.prefix}fdo-uks-api"
  enabled                        = true
}

resource "azurerm_cdn_frontdoor_origin" "frontdoor-admin-origin" {
  cdn_frontdoor_origin_group_id  = azurerm_cdn_frontdoor_origin_group.frontdoor-origin-group.id
  certificate_name_check_enabled = false
  host_name                      = azurerm_linux_web_app.gettoananswer-admin.default_hostname
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = azurerm_linux_web_app.gettoananswer-admin.default_hostname
  priority                       = 1
  weight                         = 1
  name                           = "${var.prefix}fdo-uks-admin"
  enabled                        = true
}

resource "azurerm_cdn_frontdoor_origin" "frontdoor-frontend-origin" {
  cdn_frontdoor_origin_group_id  = azurerm_cdn_frontdoor_origin_group.frontdoor-origin-group.id
  certificate_name_check_enabled = false
  host_name                      = azurerm_linux_web_app.gettoananswer-frontend.default_hostname
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = azurerm_linux_web_app.gettoananswer-frontend.default_hostname
  priority                       = 1
  weight                         = 1
  name                           = "${var.prefix}fdo-uks-frontend"
  enabled                        = true
}

// Endpoints
resource "azurerm_cdn_frontdoor_endpoint" "frontdoor-api-endpoint" {
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.id
  name                     = "${var.prefix}fde-uks-api"
  tags = {
    Environment = var.env
    Product     = var.product
  }
}

resource "azurerm_cdn_frontdoor_endpoint" "frontdoor-admin-endpoint" {
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.id
  name                     = "${var.prefix}fde-uks-admin"
  tags = {
    Environment = var.env
    Product     = var.product
  }
}

resource "azurerm_cdn_frontdoor_endpoint" "frontdoor-frontend-endpoint" {
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.id
  name                     = "${var.prefix}fde-uks-frontend"
  tags = {
    Environment = var.env
    Product     = var.product
  }
}

// Routes (separate endpoints/domains, so /* per route)
resource "azurerm_cdn_frontdoor_route" "frontdoor-api-route" {
  name                          = "${var.prefix}fdr-uks-api"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.frontdoor-api-endpoint.id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.frontdoor-origin-group.id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.frontdoor-api-origin.id]
  cdn_frontdoor_rule_set_ids    = [azurerm_cdn_frontdoor_rule_set.security_redirects.id, azurerm_cdn_frontdoor_rule_set.security_headers.id]
  enabled                       = true

  forwarding_protocol    = "MatchRequest"
  https_redirect_enabled = true
  patterns_to_match      = ["/*"]
  supported_protocols    = ["Http", "Https"]

  cdn_frontdoor_custom_domain_ids = var.api_custom_domain != "" ? [azurerm_cdn_frontdoor_custom_domain.fd-api-custom-domain[0].id] : null
  link_to_default_domain          = false
}

resource "azurerm_cdn_frontdoor_route" "frontdoor-admin-route" {
  name                          = "${var.prefix}fdr-uks-admin"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.frontdoor-admin-endpoint.id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.frontdoor-origin-group.id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.frontdoor-admin-origin.id]
  cdn_frontdoor_rule_set_ids    = [azurerm_cdn_frontdoor_rule_set.security_redirects.id, azurerm_cdn_frontdoor_rule_set.security_headers.id]
  enabled                       = true

  forwarding_protocol    = "MatchRequest"
  https_redirect_enabled = true
  patterns_to_match      = ["/*"]
  supported_protocols    = ["Http", "Https"]

  cdn_frontdoor_custom_domain_ids = var.admin_custom_domain != "" ? [azurerm_cdn_frontdoor_custom_domain.fd-admin-custom-domain[0].id] : null
  link_to_default_domain          = false
}

resource "azurerm_cdn_frontdoor_route" "frontdoor-frontend-route" {
  name                          = "${var.prefix}fdr-uks-frontend"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.frontdoor-frontend-endpoint.id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.frontdoor-origin-group.id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.frontdoor-frontend-origin.id]
  cdn_frontdoor_rule_set_ids    = [azurerm_cdn_frontdoor_rule_set.security_redirects.id, azurerm_cdn_frontdoor_rule_set.security_headers.id]
  enabled                       = true

  forwarding_protocol    = "MatchRequest"
  https_redirect_enabled = true
  patterns_to_match      = ["/*"]
  supported_protocols    = ["Http", "Https"]

  cdn_frontdoor_custom_domain_ids = var.frontend_custom_domain != "" ? [azurerm_cdn_frontdoor_custom_domain.fd-frontend-custom-domain[0].id] : null
  link_to_default_domain          = false
}

// WAF attach to all three endpoints and optional custom domains
resource "azurerm_cdn_frontdoor_security_policy" "frontdoor-web-security-policy" {
  name                     = "${var.prefix}fds-uks-security-policy"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.id

  security_policies {
    firewall {
      cdn_frontdoor_firewall_policy_id = azurerm_cdn_frontdoor_firewall_policy.web_firewall_policy.id

      association {
        domain {
          cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_endpoint.frontdoor-api-endpoint.id
        }
        domain {
          cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_endpoint.frontdoor-admin-endpoint.id
        }
        domain {
          cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_endpoint.frontdoor-frontend-endpoint.id
        }

        dynamic "domain" {
          for_each = var.api_custom_domain != "" ? ["apply"] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.fd-api-custom-domain[0].id
          }
        }

        dynamic "domain" {
          for_each = var.admin_custom_domain != "" ? ["apply"] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.fd-admin-custom-domain[0].id
          }
        }

        dynamic "domain" {
          for_each = var.frontend_custom_domain != "" ? ["apply"] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.fd-frontend-custom-domain[0].id
          }
        }

        patterns_to_match = ["/*"]
      }
    }
  }
}

// Rule sets
resource "azurerm_cdn_frontdoor_rule_set" "security_headers" {
  name                     = "${var.prefix}SecurityHeaders"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.id
}

resource "azurerm_cdn_frontdoor_rule" "security_headers_rule" {
  depends_on = [
    azurerm_cdn_frontdoor_origin_group.frontdoor-origin-group,
    azurerm_cdn_frontdoor_origin.frontdoor-api-origin,
    azurerm_cdn_frontdoor_origin.frontdoor-admin-origin,
    azurerm_cdn_frontdoor_origin.frontdoor-frontend-origin
  ]

  name                      = "securityHeaders"
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.security_headers.id
  order                     = 0
  behavior_on_match         = "Continue"

  actions {
    response_header_action {
      header_action = "Overwrite"
      header_name   = "Strict-Transport-Security"
      value         = "max-age=31536000; includeSubDomains; preload"
    }
    response_header_action {
      header_action = "Overwrite"
      header_name   = "X-Content-Type-Options"
      value         = "nosniff"
    }
  }
}


resource "azurerm_cdn_frontdoor_rule_set" "security_redirects" {
  name                     = "${var.prefix}SecurityRedirects"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.id
}

resource "azurerm_cdn_frontdoor_rule" "security_txt_rule" {
  depends_on = [
    azurerm_cdn_frontdoor_origin_group.frontdoor-origin-group,
    azurerm_cdn_frontdoor_origin.frontdoor-api-origin,
    azurerm_cdn_frontdoor_origin.frontdoor-admin-origin,
    azurerm_cdn_frontdoor_origin.frontdoor-frontend-origin
  ]

  name                      = "securityTxtRedirect"
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.security_redirects.id
  order                     = 0
  behavior_on_match         = "Continue"

  conditions {
    url_path_condition {
      operator     = "BeginsWith"
      match_values = [".well-known/security.txt", "security.txt"]
      transforms   = ["Lowercase"]
    }
  }

  actions {
    url_redirect_action {
      redirect_type        = "PermanentRedirect"
      redirect_protocol    = "Https"
      destination_hostname = "vdp.security.education.gov.uk"
      destination_path     = "/security.txt"
    }
  }
}

resource "azurerm_cdn_frontdoor_rule" "thanks_txt_rule" {
  depends_on = [
    azurerm_cdn_frontdoor_origin_group.frontdoor-origin-group,
    azurerm_cdn_frontdoor_origin.frontdoor-api-origin,
    azurerm_cdn_frontdoor_origin.frontdoor-admin-origin,
    azurerm_cdn_frontdoor_origin.frontdoor-frontend-origin
  ]

  name                      = "thanksTxtRedirect"
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.security_redirects.id
  order                     = 1
  behavior_on_match         = "Continue"

  conditions {
    url_path_condition {
      operator     = "BeginsWith"
      match_values = [".well-known/thanks.txt", "thanks.txt"]
      transforms   = ["Lowercase"]
    }
  }

  actions {
    url_redirect_action {
      redirect_type        = "PermanentRedirect"
      redirect_protocol    = "Https"
      destination_hostname = "vdp.security.education.gov.uk"
      destination_path     = "/thanks.txt"
    }
  }
}

// Custom domains (optional per variable)
resource "azurerm_cdn_frontdoor_custom_domain" "fd-api-custom-domain" {
  count                    = var.api_custom_domain != "" ? 1 : 0
  name                     = "${var.prefix}fdd-uks-api"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.id
  host_name                = var.api_custom_domain

  tls {
    certificate_type = "ManagedCertificate"
  }
}

resource "azurerm_cdn_frontdoor_custom_domain" "fd-admin-custom-domain" {
  count                    = var.admin_custom_domain != "" ? 1 : 0
  name                     = "${var.prefix}fdd-uks-admin"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.id
  host_name                = var.admin_custom_domain

  tls {
    certificate_type = "ManagedCertificate"
  }
}

resource "azurerm_cdn_frontdoor_custom_domain" "fd-frontend-custom-domain" {
  count                    = var.frontend_custom_domain != "" ? 1 : 0
  name                     = "${var.prefix}fdd-uks-frontend"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.id
  host_name                = var.frontend_custom_domain

  tls {
    certificate_type = "ManagedCertificate"
  }
}

// Associate custom domains to their routes
resource "azurerm_cdn_frontdoor_custom_domain_association" "api-app-custom-domain" {
  count                          = var.api_custom_domain != "" ? 1 : 0
  cdn_frontdoor_custom_domain_id = azurerm_cdn_frontdoor_custom_domain.fd-api-custom-domain[0].id
  cdn_frontdoor_route_ids        = [azurerm_cdn_frontdoor_route.frontdoor-api-route.id]
}

resource "azurerm_cdn_frontdoor_custom_domain_association" "admin-app-custom-domain" {
  count                          = var.admin_custom_domain != "" ? 1 : 0
  cdn_frontdoor_custom_domain_id = azurerm_cdn_frontdoor_custom_domain.fd-admin-custom-domain[0].id
  cdn_frontdoor_route_ids        = [azurerm_cdn_frontdoor_route.frontdoor-admin-route.id]
}

resource "azurerm_cdn_frontdoor_custom_domain_association" "frontend-app-custom-domain" {
  count                          = var.frontend_custom_domain != "" ? 1 : 0
  cdn_frontdoor_custom_domain_id = azurerm_cdn_frontdoor_custom_domain.fd-frontend-custom-domain[0].id
  cdn_frontdoor_route_ids        = [azurerm_cdn_frontdoor_route.frontdoor-frontend-route.id]
}

// WAF policy
resource "azurerm_cdn_frontdoor_firewall_policy" "web_firewall_policy" {
  name                = "webFirewallPolicy"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  tags = {
    Environment = var.env
    Product     = var.product
  }
  mode     = "Prevention"
  sku_name = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.sku_name
  redirect_url = coalesce(
    var.frontend_custom_domain != "" ? "https://${var.frontend_custom_domain}/en/service-unavailable" : null,
    var.admin_custom_domain != "" ? "https://${var.admin_custom_domain}/en/service-unavailable" : null,
    var.api_custom_domain != "" ? "https://${var.api_custom_domain}/en/service-unavailable" : null,
    null
  )

  dynamic "managed_rule" {
    for_each = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.sku_name == "Premium_AzureFrontDoor" ? [0] : []
    content {
      type    = "Microsoft_DefaultRuleSet"
      version = "2.1"
      action  = "Block"

      exclusion {
        match_variable = "RequestCookieNames"
        operator       = "Equals"
        selector       = "_ScCbts"
      }
    }
  }

  dynamic "managed_rule" {
    for_each = azurerm_cdn_frontdoor_profile.frontdoor-web-profile.sku_name == "Premium_AzureFrontDoor" ? [0] : []
    content {
      type    = "Microsoft_BotManagerRuleSet"
      version = "1.1"
      action  = "Block"
    }
  }

  custom_rule {
    name     = "allowcontentful"
    enabled  = true
    action   = "Allow"
    type     = "MatchRule"
    priority = 100

    match_condition {
      match_variable = "RequestHeader"
      selector       = "X-Contentful-CRN"
      operator       = "Contains"
      match_values   = ["crn:contentful"]
    }
  }

  custom_rule {
    name     = "blockarchiving"
    enabled  = true
    action   = "Block"
    type     = "MatchRule"
    priority = 150

    match_condition {
      match_variable = "RequestHeader"
      selector       = "User-Agent"
      operator       = "Contains"
      transforms     = ["Lowercase"]
      match_values   = ["mirrorweb"]
    }

    match_condition {
      match_variable = "RequestUri"
      operator       = "Contains"
      transforms     = ["Lowercase", "UrlDecode"]
      match_values   = ["/pdf/", "/translate-this-website/"]
    }
  }

  custom_rule {
    name     = "allowsearchengines"
    enabled  = true
    action   = "Allow"
    type     = "MatchRule"
    priority = 200

    match_condition {
      match_variable = "RequestHeader"
      selector       = "User-Agent"
      operator       = "RegEx"
      transforms     = ["Lowercase", "UrlDecode"]
      match_values   = ["aolbuild|baidu|bingbot|bingpreview|msnbot|duckduckgo|-google|googlebot|google-|googleother|read-aloud|teoma|slurp|yandex|yahoo"]
    }

    match_condition {
      match_variable     = "RequestUri"
      operator           = "Contains"
      negation_condition = true
      transforms         = ["Lowercase", "UrlDecode"]
      match_values       = ["/pdf/", "/translate-this-website/"]
    }
  }

  custom_rule {
    name     = "allowtools"
    enabled  = true
    action   = "Allow"
    type     = "MatchRule"
    priority = 210

    match_condition {
      match_variable = "RequestHeader"
      selector       = "User-Agent"
      operator       = "Contains"
      transforms     = ["Lowercase", "UrlDecode"]
      match_values   = ["slack", "embedly", "figma", "skype"]
    }
  }

  custom_rule {
    name     = "allowsocialmedia"
    enabled  = true
    action   = "Allow"
    type     = "MatchRule"
    priority = 220

    match_condition {
      match_variable = "RequestHeader"
      selector       = "User-Agent"
      operator       = "RegEx"
      transforms     = ["Lowercase", "UrlDecode"]
      match_values   = ["facebookbot|facebookexternalhit|facebookscraper|twitterbot|meta-externalagent|meta-externalfetcher|microsoftpreview|linkedinbot|pinterest|redditbot|telegrambot|mastadon|duckduckbot"]
    }
  }

  custom_rule {
    name     = "allowai"
    enabled  = true
    action   = "Allow"
    type     = "MatchRule"
    priority = 230

    match_condition {
      match_variable = "RequestHeader"
      selector       = "User-Agent"
      operator       = "RegEx"
      transforms     = ["Lowercase", "UrlDecode"]
      match_values   = ["oai-search|chatgpt|gptbot|cohere-ai|google-extended|amazonbot|applebot|duckassistbot"]
    }

    match_condition {
      match_variable     = "RequestUri"
      operator           = "Contains"
      negation_condition = true
      transforms         = ["Lowercase", "UrlDecode"]
      match_values       = ["/pdf/", "/translate-this-website/"]
    }
  }

  custom_rule {
    name     = "blocknonuk"
    enabled  = true
    action   = "Redirect"
    type     = "MatchRule"
    priority = 400

    match_condition {
      match_variable     = "SocketAddr"
      operator           = "GeoMatch"
      negation_condition = true
      match_values       = ["GB", "ZZ"]
    }

    match_condition {
      match_variable     = "RequestUri"
      operator           = "RegEx"
      negation_condition = true
      transforms         = ["Lowercase", "UrlDecode"]
      match_values       = ["\\/(robots\\.txt|error|service-unavailable|accessibility-statement|page-not-found|cookie-policy|privacy-policies|assets\\/|css\\/|js\\/|sitemap)"]
    }
  }
}