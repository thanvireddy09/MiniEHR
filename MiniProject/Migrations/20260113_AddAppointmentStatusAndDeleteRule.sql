/* Migration SQL helper: add Status column and a delete trigger to prevent deleting Scheduled appointments.
   Run manually against the target database or incorporate into a migration.
*/

IF NOT EXISTS (SELECT * FROM sys.columns WHERE [object_id] = OBJECT_ID(N'[Healthcare].[Appointment]') AND name = 'Status')
BEGIN
    ALTER TABLE [Healthcare].[Appointment]
    ADD [Status] NVARCHAR(50) NOT NULL CONSTRAINT DF_Appointment_Status DEFAULT('Scheduled');
END
GO

-- Create a trigger to prevent deletion of Scheduled appointments
IF OBJECT_ID('[Healthcare].[trg_PreventDeleteScheduledAppointment]', 'TR') IS NOT NULL
    DROP TRIGGER [Healthcare].[trg_PreventDeleteScheduledAppointment];
GO

CREATE TRIGGER [Healthcare].[trg_PreventDeleteScheduledAppointment]
ON [Healthcare].[Appointment]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM deleted d WHERE ISNULL(d.Status, 'Scheduled') = 'Scheduled')
    BEGIN
        RAISERROR('Cannot delete appointment with status Scheduled.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- If none of the deleted rows are Scheduled, perform the delete
    DELETE a
    FROM [Healthcare].[Appointment] a
    JOIN deleted d ON a.Id = d.Id;
END
GO
