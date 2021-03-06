﻿SELECT M."Id", M."GroupId", M."Title", M."Description", M."Status",
       AL."Id" AS AttendantListId, AL."Capacity", AL."Status" AS AttendantListStatus,
       A."Id", A."UserId", A."At", A."Waiting"
FROM "MeetupEvent" M
         LEFT JOIN "AttendantList" AL on M."Id" = AL."MeetupEventId"
         LEFT JOIN "Attendant" A on AL."Id" = A."AttendantListAggregateId";
