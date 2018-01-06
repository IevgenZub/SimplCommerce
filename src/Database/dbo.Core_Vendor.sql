alter table Core_Vendor add VendorType nvarchar(200) null
alter table Core_Vendor add VendorClass nvarchar(50) null
alter table Core_Vendor add Phone1 nvarchar(200) null
alter table Core_Vendor add Phone2 nvarchar(200) null
alter table Core_Vendor add Phone3 nvarchar(200) null
alter table Core_Vendor add Phone4 nvarchar(200) null
alter table Core_Vendor add Address nvarchar(200) null
alter table Core_Vendor add City nvarchar(200) null
alter table Core_Vendor add Area nvarchar(200) null
alter table Core_Vendor add CountryId bigint null foreign key references Core_Country(Id)
alter table Core_Vendor add Website nvarchar(400) null
alter table Core_Vendor add SendEmails bit null
alter table Core_Vendor add BankName nvarchar(200) null
alter table Core_Vendor add AccountNumber nvarchar(200) null
alter table Core_Vendor add Iban nvarchar(200) null
alter table Core_Vendor add Notes nvarchar(max) null
