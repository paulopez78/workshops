DROP DATABASE "meetup";
DROP USER "meetup";

CREATE DATABASE "meetup";
CREATE SCHEMA scheduling;

CREATE USER meetup WITH PASSWORD 'password';

GRANT ALL PRIVILEGES ON DATABASE "meetup" to meetup;
GRANT ALL PRIVILEGES ON SCHEMA "scheduling" to meetup;

SELECT * FROM meetup.scheduling.mt_streams;
SELECT * FROM meetup.scheduling.mt_events;
SELECT * FROM meetup.scheduling.mt_doc_meetupevent;
SELECT * FROM meetup.scheduling.mt_doc_attendantlist;
SELECT * FROM meetup.scheduling.mt_doc_outbox;


DELETE FROM meetup.scheduling.mt_streams;
DELETE FROM meetup.scheduling.mt_events;
DELETE FROM meetup.scheduling.mt_doc_meetupevent;
DELETE FROM meetup.scheduling.mt_doc_attendantlist;
DELETE FROM meetup.scheduling.mt_doc_outbox;
