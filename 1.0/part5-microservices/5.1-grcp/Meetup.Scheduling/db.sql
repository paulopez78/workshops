DROP DATABASE "meetup";
DROP USER "meetup";

CREATE DATABASE "meetup";
CREATE SCHEMA scheduling;

CREATE USER meetup WITH PASSWORD 'password';

GRANT ALL PRIVILEGES ON DATABASE "meetup" to meetup;
GRANT ALL PRIVILEGES ON SCHEMA "scheduling" to meetup;

SELECT * FROM meetup.scheduling.mt_streams order by timestamp desc;
SELECT * FROM meetup.scheduling.mt_events order by timestamp desc;
SELECT * FROM meetup.scheduling.mt_event_progression;
SELECT * FROM meetup.scheduling.mt_doc_attendantlistprojection_attendantlist;
SELECT * FROM meetup.scheduling.mt_doc_meetupdetailseventprojection_meetupdetailsevent;
SELECT * FROM meetup.scheduling.mt_doc_outbox;
SELECT * FROM meetup.scheduling.mt_doc_meetupevent;


DELETE FROM meetup.scheduling.mt_streams;
DELETE FROM meetup.scheduling.mt_events;
DELETE FROM meetup.scheduling.mt_doc_attendantlistprojection_attendantlist;
DELETE FROM meetup.scheduling.mt_doc_meetupdetailseventprojection_meetupdetailsevent;
DELETE FROM meetup.scheduling.mt_doc_outbox;
DELETE FROM meetup.scheduling.mt_doc_meetupevent;
DELETE FROM meetup.scheduling.mt_event_progression;