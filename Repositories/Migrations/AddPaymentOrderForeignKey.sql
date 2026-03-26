-- Run this once on your MySQL database to add the missing foreign key
-- between Payments and Orders. Then refresh your ERD and the connection will appear.
-- (A past migration dropped this FK and only re-created the index.)

ALTER TABLE `Payments`
ADD CONSTRAINT `FK_Payments_Orders_OrderId`
FOREIGN KEY (`OrderId`) REFERENCES `Orders`(`Id`) ON DELETE RESTRICT;
