# Kafka on AKS

## Strimzi Kafka Operator

    helm repo add strimzi https://strimzi.io/charts/
	helm install strimzi-kafka strimzi/strimzi-kafka-operator

## Kafka & Zookeeper

	kubectl apply -f k8s-kafka.yml

## Kakfa Connect (with Debezium MySQL, Elasticsearch, Neo4j Connectors)

    kubectl apply -f .\k8s-schema-registry.yml
	kubectl apply -f .\k8s-connect.yml

[k8s-schema-registry.yml](k8s-schema-registry.yml)    [k8s-connect.yml](k8s-connect.yml)
https://github.com/confluentinc/demo-scene/tree/master/kafka-connect-zero-to-hero

## MySQL Source Connector

	curl -i -X PUT -H  "Content-Type:application/json" \
		http://localhost:8083/connectors/source-debezium-orders-00/config \
		-d '{
				"connector.class": "io.debezium.connector.mysql.MySqlConnector",
				"database.hostname": "mysql",
				"database.port": "3306",
				"database.user": "debezium",
				"database.password": "dbz",
				"database.server.id": "42",
				"database.server.name": "asgard",
				"table.whitelist": "demo.orders",
				"database.history.kafka.bootstrap.servers": "kafkadev-kafka-external-bootstrap:9094",
				"database.history.kafka.topic": "dbhistory.demo" ,
				"decimal.handling.mode": "double",
				"include.schema.changes": "true",
				"transforms": "unwrap,addTopicPrefix",
				"transforms.unwrap.type": "io.debezium.transforms.ExtractNewRecordState",
				"transforms.unwrap.drop.tombstones": "false",
				"transforms.unwrap.delete.handling.mode": "rewrite",
				"transforms.addTopicPrefix.type":"org.apache.kafka.connect.transforms.RegexRouter",
				"transforms.addTopicPrefix.regex":"(.*)",
				"transforms.addTopicPrefix.replacement":"mysql-debezium-$1"
		}'

## Elastic Sink Connector

	curl -i -X PUT -H  "Content-Type:application/json" \
		http://localhost:8083/connectors/sink-elastic-orders-00/config \
		-d '{
			"connector.class": "io.confluent.connect.elasticsearch.ElasticsearchSinkConnector",
			"topics": "mysql-debezium-asgard.demo.ORDERS",
			"connection.url": "http://elasticdev-es-http.default.svc:9200",
			"connection.username": "logstash_writer",
			"connection.password": "UpSvQJe4mZTwvuyApky5",
			"tasks.max": "1",
			"type.name": "_doc",
			"key.ignore": "true",
			"schema.ignore": "true"
		}'

Debugging

	kubectl exec -it kafkadev-kafka-0 -- bin/kafka-topics.sh --list --zookeeper localhost:2181
	kubectl exec -i kafkadev-kafka-0 -- bin/kafka-console-consumer.sh --bootstrap-server kafkadev-kafka-bootstrap:9092 --topic mysql-debezium-asgard.demo.ORDERS --from-beginning
