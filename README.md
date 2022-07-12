# Erm Messaging

### Features

- Serialization Json|Protobuf
- Message Outbox
- Saga
- Kafka Transport
- MessageGateway (Deduplication & Retry)
- Typed Message Handler

### To run integration tests

1. install docker
2. install docker-compose
3. open terminal in `env` folder and run `make up`
4. :pray: and run tests :rocket:

### TODO

- [ ] Retry queue ([sample](https://www.confluent.io/blog/error-handling-patterns-in-kafka/))
- [ ] Dead letter queue