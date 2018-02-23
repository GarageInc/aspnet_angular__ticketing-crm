use gb_ts_staging;

LOCK TABLES `tic_TicketStatuses` WRITE;
/*!40000 ALTER TABLE `tic_TicketStatuses` DISABLE KEYS */;
INSERT INTO `tic_TicketStatuses` VALUES (1,'Open','Open'),(2,'In Progress','In Progress'),(3,'In Progress','Waiting for customer reply'),(4,'In Progress','Waiting for support reply'),(5,'Fixed','Fixed'),(6,'Closed','Closed - Didn’t have any issue'),(7,'Closed','Closed - Issue was resolved'),(8,'Closed','Closed - Irrelevant anymore'),(9,'Closed','Closed - Closed by user'),(10,'In Progress','Waiting for staff reply');
/*!40000 ALTER TABLE `tic_TicketStatuses` ENABLE KEYS */;
UNLOCK TABLES;