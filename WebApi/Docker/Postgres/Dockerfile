FROM postgres

RUN mkdir data

COPY setup.sql ./docker-entrypoint-initdb.d
COPY data.csv ./data

ENV POSTGRES_DB=TestDB
ENV POSTGRES_USER=TestDev
ENV POSTGRES_PASSWORD=Xj7hgrw@hn!49
