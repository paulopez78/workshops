DROP DATABASE "meetup";
DROP USER "meetup";

CREATE DATABASE "meetup";
CREATE SCHEMA scheduling;

CREATE USER meetup WITH PASSWORD 'password';
GRANT ALL PRIVILEGES ON DATABASE "meetup" to meetup;

SELECT * FROM meetup.scheduling.mt_streams order by timestamp desc;
SELECT * FROM meetup.scheduling.mt_events order by timestamp desc;
SELECT * FROM meetup.scheduling.mt_event_progression;
SELECT * FROM meetup.scheduling.mt_doc_v1_meetupevent;
SELECT * FROM meetup.scheduling.mt_doc_v1_meetupdetailseventreadmodel;
SELECT * FROM meetup.scheduling.mt_doc_v1_attendantlistreadmodel;
SELECT * FROM meetup.scheduling.mt_doc_outbox;

DELETE FROM meetup.scheduling.mt_streams;
DELETE FROM meetup.scheduling.mt_events;
DELETE FROM meetup.scheduling.mt_event_progression;
DELETE FROM meetup.scheduling.mt_doc_v1_meetupdetailseventreadmodel;
DELETE FROM meetup.scheduling.mt_doc_v1_attendantlistreadmodel;
DELETE FROM meetup.scheduling.mt_doc_v1_meetupevent;
DELETE FROM meetup.scheduling.mt_doc_outbox;
