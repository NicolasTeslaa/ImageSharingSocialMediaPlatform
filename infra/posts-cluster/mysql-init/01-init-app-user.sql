CREATE DATABASE IF NOT EXISTS ImageSharingPostsDb;
CREATE DATABASE IF NOT EXISTS ImageSharingTimelineDb;
CREATE DATABASE IF NOT EXISTS ImageSharingUsersDb;

CREATE USER IF NOT EXISTS 'posts_app'@'%' IDENTIFIED BY 'posts_app_123';
GRANT ALL PRIVILEGES ON ImageSharingPostsDb.* TO 'posts_app'@'%';
GRANT ALL PRIVILEGES ON ImageSharingTimelineDb.* TO 'posts_app'@'%';
GRANT ALL PRIVILEGES ON ImageSharingUsersDb.* TO 'posts_app'@'%';
FLUSH PRIVILEGES;
