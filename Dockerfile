name: Deploy C#

on:
  push:
    branches:
      - develop
    paths:
      - '**/*.cs'
      - '**/*.csproj'
      - 'Dockerfile'

env:
  IMAGE_NAME: davidvothe/appointment

jobs:
  deploy:
    runs-on: ubuntu-latest

    steps:
    - name: Checkout code
      uses: actions/checkout@v3

    - name: Docker login
      uses: docker/login-action@v3
      with:
        username: ${{ secrets.DOCKER_USERNAME }}
        password: ${{ secrets.DOCKER_PASSWORD }}

    - name: Build & Push Docker image
      run: |
        docker build -t $IMAGE_NAME:latest .
        docker push $IMAGE_NAME:latest

    - name: Deploy to VPS via SSH
      uses: appleboy/ssh-action@v1.0.3
      with:
        host: ${{ secrets.VPS_HOST }}
        username: ${{ secrets.VPS_USER }}
        key: ${{ secrets.VPS_SSH_KEY }}
        script: |
          docker pull $IMAGE_NAME:latest
          docker stop backend || true
          docker rm backend || true
          docker run -d --name backend -p 80:80 $IMAGE_NAME:latest
