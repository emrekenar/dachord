#!/bin/bash
echo "----------- INITIALIZING DYNAMODB -----------"

awslocal dynamodb create-table \
  --table-name Tracks \
  --attribute-definitions AttributeName=Id,AttributeType=S AttributeName=Type,AttributeType=S AttributeName=Email,AttributeType=S \
  --key-schema AttributeName=Id,KeyType=HASH AttributeName=Type,KeyType=RANGE \
  --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
  --global-secondary-indexes '[{"IndexName":"EmailIndex","KeySchema":[{"AttributeName":"Email","KeyType":"HASH"}],"Projection":{"ProjectionType":"ALL"},"ProvisionedThroughput":{"ReadCapacityUnits":5,"WriteCapacityUnits":5}}]'

echo "----------- TRACKS TABLE CREATED -----------"