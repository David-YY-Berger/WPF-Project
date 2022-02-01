public static class FactoryDL
{
    public static DalXml.IDal GetDL(string libraryType)
    {
        if (libraryType == "a")
            return DalXml.DataSource.Instance;
        //else if (libraryType == "b")
        //    return DalObject.DalApi.DataSource.Instance;
        //else
            throw new DalXml.DO.EXItemNotFoundException();
    }
}


