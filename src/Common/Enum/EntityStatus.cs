namespace Common.Enum;

public enum EntityStatus
{
    Draft = 1,
    Published = 2,
    Deleted = 3,
    Archived = 4,
    
    /**
     * Replaced with "boolean IsUnpublished" for future changes 
     */
    Private = 5,
}