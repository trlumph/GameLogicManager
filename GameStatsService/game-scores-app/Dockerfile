FROM python:3.10-slim

WORKDIR /app

COPY . /app

RUN pip install --no-cache-dir -r requirements.txt

ENV CASSANDRA_HOSTS cassandra-0,cassandra-1,cassandra-2
ENV CASSANDRA_PORT 9042
ENV CASSANDRA_KEYSPACE game_stats

CMD ["uvicorn", "rest_application:app", "--host", "0.0.0.0", "--port", "8080", "--reload"]
