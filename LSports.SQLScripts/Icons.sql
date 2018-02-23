use gb_ts_staging;

LOCK TABLES `tic_Icons` WRITE;
/*!40000 ALTER TABLE `tic_Icons` DISABLE KEYS */;
INSERT INTO `tic_Icons` VALUES (1,'fa-building','building'),(2,'fa-bug','bug'),(3,'fa-calendar','calendar'),(4,'fa-envelope','envelope'),(5,'fa-eye','eye'),(6,'fa-flag','flag'),(7,'fa-info','info');
/*!40000 ALTER TABLE `tic_Icons` ENABLE KEYS */;
UNLOCK TABLES;