# ESEP Webhooks

This project implements an event-driven architecture that connects GitHub Issues to a Slack channel using AWS Lambda and API Gateway. When a new GitHub Issue is created, a webhook triggers a Lambda function that sends the issue details to Slack.


## Overview

The system demonstrates asynchronous communication using webhooks. GitHub emits an event when an issue is created, API Gateway receives the event, and Lambda processes the payload before forwarding a formatted message to Slack through an incoming webhook URL stored as an environment variable.


## Architecture

**Workflow:**

1. **GitHub Webhook:** Configured to trigger only on “Issues” events.
2. **API Gateway (REST):** Accepts HTTP POST requests from GitHub and forwards them to Lambda.
3. **AWS Lambda:** Processes the GitHub payload and posts to Slack.
4. **Slack:** Receives and displays formatted issue data.


## Setup Instructions

### 1. AWS IAM and CLI

* Create an IAM user with `AWSLambda_FullAccess` and `IAMFullAccess` permissions.
* Configure the AWS CLI using:

  ```bash
  aws configure
  ```

  Region: `us-east-2`
  Output format: `json`

### 2. Lambda Deployment

Create and deploy the Lambda function using .NET:

```bash
dotnet new lambda.EmptyFunction --name EsepWebhook
dotnet lambda deploy-function EsepWebhook --region us-east-2
```

### 3. Environment Variable

Add the Slack webhook URL as an environment variable in the Lambda console:

* **Key:** `SLACK_URL`
* **Value:** (Provided Slack incoming webhook URL)

Do not include this value in your repository.


## API Gateway Configuration

1. Create a **REST API** (Open security, Regional endpoint).
2. Under `/`, add a **POST** method integrated with the Lambda function.
3. Enable **Lambda proxy integration**.
4. Deploy the API with stage name `default`.
5. Copy the **Invoke URL** (e.g. `https://xxxx.execute-api.us-east-2.amazonaws.com/default`).


## GitHub Webhook Setup

1. Go to **Repository → Settings → Webhooks → Add Webhook**.
2. **Payload URL:** your API Gateway Invoke URL.
3. **Content type:** `application/json`.
4. **Events:** “Let me select individual events” → select **Issues** only.
5. Click **Add Webhook**.


## Testing

1. Create a new GitHub Issue in the repository.
2. Verify that a corresponding message appears in the designated Slack channel.

If the Slack message does not appear:

* Check **GitHub → Webhooks → Recent Deliveries** for status `200`.
* Review **CloudWatch Logs** for the Lambda function for possible parsing or environment variable errors.


## Deployment Commands

```bash
dotnet add package Newtonsoft.Json
dotnet lambda deploy-function EsepWebhook --region us-east-2
```


## Repository Contents

```
/EsepWebhook
 ├── src/
 │    └── EsepWebhook/
 │         ├── Function.cs
 │         ├── EsepWebhook.csproj
 │         └── ...
 ├── test/
 │    └── EsepWebhook.Tests/
 └── README.md
```