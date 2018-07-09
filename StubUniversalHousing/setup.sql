CREATE DATABASE StubUH;
GO
USE StubUH;
GO
CREATE TABLE tenagree (tag_ref NVARCHAR(MAX), cur_bal FLOAT);
CREATE TABLE araction (tag_ref NVARCHAR(MAX), action_code NVARCHAR(MAX), action_date SMALLDATETIME);
CREATE TABLE arag (tag_ref NVARCHAR(MAX), arag_status NVARCHAR(MAX));
GO