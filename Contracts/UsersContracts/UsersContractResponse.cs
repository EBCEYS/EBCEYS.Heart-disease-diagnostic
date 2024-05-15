namespace UsersContracts
{
    public class UsersContractResponse<T>
    {
        public UsersResponseContractResult Result { get; set; }
        public T? Object { get; set; }
    }
}