#!/bin/bash
echo "----------- INITIALIZING DYNAMODB -----------"

awslocal dynamodb create-table \
  --table-name dachord-dev-tracks \
  --attribute-definitions AttributeName=pk,AttributeType=S AttributeName=sk,AttributeType=S \
  --key-schema AttributeName=pk,KeyType=HASH AttributeName=sk,KeyType=RANGE \
  --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5

awslocal dynamodb create-table \
  --table-name dachord-dev-users \
  --attribute-definitions AttributeName=pk,AttributeType=S AttributeName=Email,AttributeType=S \
  --key-schema AttributeName=pk,KeyType=HASH \
  --global-secondary-indexes '[{"IndexName":"EmailIndex","KeySchema":[{"AttributeName":"Email","KeyType":"HASH"}],"Projection":{"ProjectionType":"ALL"},"ProvisionedThroughput":{"ReadCapacityUnits":5,"WriteCapacityUnits":5}}]' \
  --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5

echo "----------- TABLES CREATED -----------"
