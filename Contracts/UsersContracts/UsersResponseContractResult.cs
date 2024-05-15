namespace UsersContracts
{
    public enum UsersResponseContractResult
    {
        Error,
        Ok,
        UserNotFound,
        UserAlreadyExists,
        RoleDoesNotExist,
        RoleAlreadyExists
    }
}