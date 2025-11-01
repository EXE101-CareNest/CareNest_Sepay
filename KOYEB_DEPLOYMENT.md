# Deploying CareNest_SePay to Koyeb

This guide explains how to deploy the CareNest_SePay service to Koyeb.

## Prerequisites

- Docker installed locally
- A Docker Hub account (or other container registry)
- A Koyeb account
- Your SePay API credentials

## Step 1: Build and Push the Docker Image

```powershell
# Login to Docker Hub
docker login

# Build the image
docker build -t yourusername/carenest-sepay:latest .

# Push to Docker Hub
docker push yourusername/carenest-sepay:latest
```

## Step 2: Create a Koyeb App

1. Go to the Koyeb dashboard
2. Click "Create App"
3. Choose "Docker Image" as the deployment method
4. Enter your image: `yourusername/carenest-sepay:latest`
5. Configure the port: 8080

## Step 3: Configure Environment Variables

Add these environment variables in Koyeb App settings:

### Required Environment Variables

```plaintext
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

### Required Secrets
Add these as encrypted secrets:

```plaintext
ConnectionStrings__PostgresConnection=Host=your-db-host;Port=5432;Username=your-username;Password=your-password;Database=your-database;
Sepay__ApiKey=your-sepay-api-key
Sepay__SecretKey=your-sepay-secret-key
```

### Optional Configuration
```plaintext
Sepay__BaseUrl=https://api.sepay.vn
Sepay__WebhookUrl=/api/sepay/webhook
APIService__BaseUrlOrder=your-order-service-url
```

## Step 4: Configure SePay Webhook

After deployment, update your SePay console settings:
1. Log in to SePay dashboard
2. Set Webhook URL to: `https://your-app-name.koyeb.app/api/sepay/webhook`
3. Ensure the webhook is using your configured API key for authentication

## Step 5: Verify Deployment

1. Check the app logs in Koyeb dashboard
2. Test the health endpoint: `https://your-app-name.koyeb.app/health`
3. Test webhook using the sandbox endpoint: `POST https://your-app-name.koyeb.app/api/sepay/test/sandbox`

## Troubleshooting

### Common Issues

1. **Database Connection**
   - Ensure your database is accessible from Koyeb
   - Check the connection string format
   - Verify database credentials

2. **Webhook Not Receiving**
   - Verify SePay webhook URL is correct
   - Check Authorization header matches Sepay__ApiKey
   - Check Koyeb logs for webhook requests

3. **Container Not Starting**
   - Check Koyeb logs for startup errors
   - Verify all required environment variables are set
   - Check the health check status

## Monitoring

- Use Koyeb dashboard to monitor:
  - Container health status
  - Application logs
  - Resource usage

## Security Notes

- Never commit sensitive values to the repository
- Use Koyeb Secrets for sensitive data
- Regularly rotate API keys and secrets
- Monitor webhook endpoints for unauthorized access attempts