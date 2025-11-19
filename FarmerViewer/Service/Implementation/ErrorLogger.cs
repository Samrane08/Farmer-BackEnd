using Repository.Data;
using Service.Interface;

namespace Service.Implementation
{
    public class ErrorLogger: IErrorLogger
    {
        private readonly ApplicationDbContext context;

        public ErrorLogger(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task Log(string LogAt,Exception ex)
        {
            try
            {
                string error = string.Empty;
                error += "Message ---\n{0}" + ex.Message;
                error += Environment.NewLine + "Source ---\n{0}" + ex.Source;
                error += Environment.NewLine + "StackTrace ---\n{0}" + ex.StackTrace;
                error += Environment.NewLine + "TargetSite ---\n{0}" + ex.TargetSite;
                if (ex.InnerException != null)
                {
                    error += Environment.NewLine + "Inner Exception is {0}" + ex.InnerException;
                }
                if (ex.HelpLink != null)
                {
                    error += Environment.NewLine + "HelpLink ---\n{0}" + ex.HelpLink;
                }
                var entity = new Repository.Entity.ErrorLogger()
                {
                    Exception = error,
                    CreatedOn = DateTime.Now,
                    ErrorAt = LogAt
                };
                await context.ErrorLogger.AddAsync(entity);   
                await context.SaveChangesAsync();   
            }
            catch (Exception)
            {
               
            }
        }
    }
}
