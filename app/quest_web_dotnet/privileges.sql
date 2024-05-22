CREATE USER 'application'@'localhost' IDENTIFIED BY 'password';
GRANT ALL PRIVILEGES ON *.* TO 'application'@'localhost';
FLUSH PRIVILEGES;