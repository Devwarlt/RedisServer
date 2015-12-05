﻿using common;

namespace server.account
{
    internal class verify : RequestHandler
    {
        protected override void HandleRequest()
        {
            DbAccount acc;
            var status = Database.Verify(Query["guid"], Query["password"], out acc);
            if (status == LoginStatus.OK)
                WriteLine(Account.FromDb(acc).ToXml().ToString());
            else
                WriteErrorLine(status.GetInfo());
        }
    }
}
