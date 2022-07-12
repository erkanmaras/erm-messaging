.PHONY: up down down-v up-aws-test

base_compose_file="docker-compose.base.yml"
dev_compose_file="docker-compose.dev.yml"
aws_test_compose_file="docker-compose.aws-test.yml"

up:	
	@echo command | date
	@echo initializing ${dev_compose_file}
	docker-compose -f ${base_compose_file} -f ${dev_compose_file} up -d
 	
down:
	@echo command | date
	@echo shutting down ${dev_compose_file}
	docker-compose -f ${base_compose_file} -f ${dev_compose_file} down

down-v:
	@echo command | date
	@echo shutting down ${dev_compose_file}
	docker-compose -f ${base_compose_file} -f ${dev_compose_file} down -v

# <- Attention !!! this command used by aws-codebuild ->
up-aws-test:	
	@echo command | date
	@echo initializing ${aws_test_compose_file}
	docker-compose -f ${base_compose_file} -f ${aws_test_compose_file} up -d
# ->
 	
 