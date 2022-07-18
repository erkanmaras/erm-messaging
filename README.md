# Erm Messaging

### Features

- [Incoming/Outgoing Message Pipeline](https://github.com/erkanmaras/erm-messaging/tree/main/src/Messaging/src/Erm.Messaging)
- [Serialization Json|Protobuf](https://github.com/erkanmaras/erm-messaging/tree/main/src/Messaging/src/Erm.Messaging.Serialization.Protobuf)
- [Message Outbox](https://github.com/erkanmaras/erm-messaging/tree/main/src/Outbox/src)
- [Saga](https://github.com/erkanmaras/erm-messaging/tree/main/src/Saga/src)
- [Kafka Transport](https://github.com/erkanmaras/erm-messaging/tree/main/src/Transports/KafkaTransport/src/Erm.Messaging.KafkaTransport)
- [MessageGateway (Deduplication & Retry)](https://github.com/erkanmaras/erm-messaging/tree/main/src/MessageGateway/src)
- [Typed Message Handler](https://github.com/erkanmaras/erm-messaging/tree/main/src/Messaging/src/Erm.Messaging.TypedMessageHandler)

### To run integration tests

1. install docker
2. install docker-compose
3. open terminal in `env` folder and run `make up`
4. :pray: and run tests :rocket:

### TODO

- [ ] Retry queue ([sample](https://www.confluent.io/blog/error-handling-patterns-in-kafka/))
- [ ] Dead letter queue
