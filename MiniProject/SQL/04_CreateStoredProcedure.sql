USE EHR;
GO

-- Stored Procedure: sp_CreateAppointment
-- Creates an appointment from C#
CREATE OR ALTER PROCEDURE [Healthcare].[sp_CreateAppointment]
    @PatientId INT,
    @AppointmentDate DATETIME2,
    @Reason NVARCHAR(255),
    @DoctorId INT = NULL,
    @Status NVARCHAR(50) = 'Scheduled',
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [Healthcare].[Appointment] (PatientId, AppointmentDate, Reason, Status, DoctorId)
    VALUES (@PatientId, @AppointmentDate, @Reason, @Status, @DoctorId);

    SET @NewId = SCOPE_IDENTITY();
END
GO
