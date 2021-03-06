﻿/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using DCEMV.ServerShared;
using DCEMV.Shared;
using DCEMV.TLVProtocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DCEMV.DemoApp.Proxies
{
    public class DCEMVServerOnlineApprover : IOnlineApprover
    {
        public static Logger Logger = new Logger(typeof(DCEMVServerOnlineApprover));

        public ApproverResponseBase DoReversal(ApproverRequestBase request, bool isOnline)
        {
            throw new NotImplementedException();
        }

        public ApproverResponseBase DoAdvice(ApproverRequestBase request, bool isOnline)
        {
            throw new NotImplementedException();
        }

        public ApproverResponseBase DoCheckAuthStatus(ApproverRequestBase request)
        {
            throw new NotImplementedException();
        }
        public ApproverResponseBase DoAuth(ApproverRequestBase request)
        {

            if (request is EMVApproverRequest)
                return DoEMVAuth(request);
            if (request is QRCodeApproverRequest)
                return DoQRAuth(request);
            else
                throw new NotImplementedException();

        }

        private ApproverResponseBase DoEMVAuth(ApproverRequestBase requestIn)
        {
            try
            {
                EMVApproverRequest request = ((EMVApproverRequest)requestIn);

                DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                using (SessionSingleton.HttpClient)
                {
                    ContactCardOnlineAuthRequest tx = new ContactCardOnlineAuthRequest()
                    {
                        EMV_Data = TLVasJSON.Convert(request.EMV_Data),
                    };
                    string responseJson = "";
                    Task.Run(async () => {
                        responseJson = await client.TransactionAuthtransactiontoissuerPostAsync(tx.ToJsonString());
                    }).Wait();
                    ContactCardOnlineAuthResponse response = ContactCardOnlineAuthResponse.FromJsonString(responseJson);

                    EMVApproverResponse approverResponse = null;
                    switch (response.Response)
                    {
                        case ContactCardOnlineAuthResponseEnum.Approved:
                        case ContactCardOnlineAuthResponseEnum.Declined:
                            approverResponse = new EMVApproverResponse();
                            approverResponse.AuthCode_8A = TLVasJSON.Convert(response.AuthCode_8A);
                            approverResponse.IssuerAuthData_91 = TLVasJSON.Convert(response.IssuerAuthData_91);
                            approverResponse.IsApproved = response.Response == ContactCardOnlineAuthResponseEnum.Approved ? true : false;
                            approverResponse.ResponseMessage = response.ResponseMessage;
                            break;

                        case ContactCardOnlineAuthResponseEnum.UnableToGoOnline:
                            break;

                    }

                    return approverResponse;
                }
            }
            catch
            {
                return null;
            }
        }
        private ApproverResponseBase DoQRAuth(ApproverRequestBase requestIn)
        {
            throw new NotImplementedException();
        }
    }
}
