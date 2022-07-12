#----------------------------------------------
#  ATTENTION !
#  This table used by external tools (debezium)
#  Dont make any breaking change without notice!
#----------------------------------------------
create table if not exists _MessageOutbox
(
    Id                 binary(16)                             not null primary key,
    MessageId          varchar(36)                            not null,
    GroupId            varchar(36),
    CorrelationId      varchar(36),
    Destination        varchar(255)                           not null,
    Time               datetime(3),
    TimeToLive         int,
    RequestId          varchar(36),
    Source             varchar(255),
    ReplyTo            varchar(255),
    ExtendedProperties json,
    MessageName        varchar(255)                           not null,
    MessageContentType varchar(64)                            not null,
    Message            mediumblob,
    CreatedAt          datetime(3) default (UTC_TIMESTAMP(3)) not null
);
#----------------------------------------------