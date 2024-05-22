FROM mysql:8.0
MAINTAINER toto1236564@gmail.com

ENV MYSQL_ROOT_PASSWORD pwd
ENV MYSQL_USER application
ENV MYSQL_PASSWORD password
ENV MYSQL_DATABASE quest_web
COPY ./storage/ /var/lib/mysql
COPY ./privileges.sql /docker-entrypoint-initdb.d/
EXPOSE 3306