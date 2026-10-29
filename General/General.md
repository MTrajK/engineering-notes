# General notes

## Webhooks
Webhook explanation
- https://zapier.com/blog/what-are-webhooks/
- https://www.redhat.com/en/topics/automation/what-is-a-webhook

It's simple just a simple API (one directional call). That is used by another APP when something happens - that another APP need to notify/call your APP, so in order to do that you're creating an API (URL) and give to that other APP.
The other APP is just executing the API (URL), and nothing else! Like calling an API witout configuration, request body, etc (it will expect just a simple parameter).

WebHook is just an idea, just a pattern how to use a web api... (nothing special or more... just how and when you're using it)

### Webhook VS API
- https://zapier.com/blog/webhook-vs-api/

APIs open the door to back-and-forth communication between software applications via requests—App A requests information from App B, and App B decides whether to send the information.

A webhook is a type of event-driven API. Rather than sending information in response to another app's request, a webhook sends information or performs a specific function in response to a trigger—like the time of day, clicking a button, or receiving a form submission. Since the application sending the data initiates the transfer, webhooks are often referred to as "reverse APIs." 