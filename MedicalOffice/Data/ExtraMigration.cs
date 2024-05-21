using Microsoft.EntityFrameworkCore.Migrations;

namespace MedicalOffice.Data
{
    public static class ExtraMigration
    {
        public static void Steps(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetPatientTimestampOnUpdate
                    AFTER UPDATE ON Patients
                    BEGIN
                        UPDATE Patients
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetPatientTimestampOnInsert
                    AFTER INSERT ON Patients
                    BEGIN
                        UPDATE Patients
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    Drop View IF EXISTS [AppointmentSummaries];
		            Create View AppointmentSummaries as
                    Select p.ID, p.FirstName, p.MiddleName, p.LastName, Count(*) as NumberOfAppointments, 
	                    Sum(a.extraFee) as TotalExtraFees, Max(a.extraFee) as MaximumFeeCharged
                    From Patients p Join Appointments a
                    on p.ID = a.PatientID
                    Group By p.ID, p.FirstName, p.MiddleName, p.LastName;
                ");
            migrationBuilder.Sql(
                @"
		            Drop View IF EXISTS [AppointmentReasonSummaries];
		            Create View AppointmentReasonSummaries as
                    Select r.ID, r.ReasonName, Count(a.ID) as NumberOfAppointments, 
                        ifnull(Avg(cast((strftime('%Y', 'now') - strftime('%Y', p.DOB)) - 
                            (strftime('%m-%d', 'now') < strftime('%m-%d', p.DOB)) as int)),0) as AverageAge,
	                    ifnull(Sum(a.extraFee),0) as TotalExtraFees, 
                        ifnull(Max(a.extraFee),0) as MaximumFeeCharged
                    From AppointmentReasons r left join Appointments a 
                        on r.ID = a.AppointmentReasonID
                        left join Patients p 
                        on p.ID = a.PatientID
                    Group By r.ID, r.ReasonName;
                ");
        }
    }
}
