namespace Microsoft.Cds.Metadata
{
  public enum OwnershipTypes
  {
    None,
    UserOwned,
    TeamOwned,
    BusinessOwned = 4,
    OrganizationOwned = 8,
    BusinessParented = 16
  }
}
