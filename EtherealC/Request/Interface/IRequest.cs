namespace EtherealC.Request.Interface
{
    public interface IRequest
    {
        #region --方法--
        void Initialize();
        void Register();
        void UnRegister();
        void UnInitialize();
        #endregion
    }
}
