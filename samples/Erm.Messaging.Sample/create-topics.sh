TOPIC_NAMES=('ping-topic' 'pong-topic')


for topicName in "${TOPIC_NAMES[@]}"
do
  echo "creating $topicName"
  docker exec messaging_kafka  bash -c "kafka-topics --create --bootstrap-server localhost:9044 --replication-factor 1 --partitions 1 --topic $topicName;"
done
