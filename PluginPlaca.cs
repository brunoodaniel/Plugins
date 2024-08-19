using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsProjetoFinal
{
    public class PluginPlaca : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.
                GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            //Verifica se o gatilho é do tipo “create “e se a entidade é do tipo “smt_cliente” 
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "smt_veiculo")
            {
                //Verifica se o contexto do formulário e dos campos 
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is
                Entity)
                {
                    Entity targetVeiculo = (Entity)context.InputParameters["Target"];

                    String placaVeiculo = (String)targetVeiculo["smt_placadoveiculo"];

                    char[] caracterePlaca = placaVeiculo.ToCharArray();

                    if (char.IsLetter(caracterePlaca[4]))
                    {
                        targetVeiculo["smt_placa"] = new OptionSetValue(2);
                    }
                    else
                    {
                        targetVeiculo["smt_placa"] = new OptionSetValue(1);
                    }
                }
            }
        }
    }
}
