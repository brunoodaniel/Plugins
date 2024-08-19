using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace PluginsProjetoFinal
{
    public class PluginVerificaPacoteFechado : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Verifica se o gatilho é do tipo "create" ou "update" e se a entidade é do tipo "smt_aluguel"
            if ((context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "smt_aluguel"))
            {
                // Verifica se o contexto do formulário e dos campos
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity targetAluguel = (Entity)context.InputParameters["Target"];

                    // Verifica a existência dos campos necessários
                    if (targetAluguel.Attributes.Contains("smt_pacotefechado") && ((bool)targetAluguel["smt_pacotefechado"]) == false)
                    {
                        if (targetAluguel.Attributes.Contains("smt_aluguelate"))
                        {
                            DateTime aluguelDe = (DateTime)targetAluguel["smt_aluguelde"];
                            DateTime aluguelAte = (DateTime)targetAluguel["smt_aluguelate"];

                            // Calcula a diferença em horas
                            int diferencaHoras = (int)(aluguelAte - aluguelDe).TotalHours;

                            // Obtém o valor diário do veículo
                            Money valordiaria = ObterValorDiarioVeiculo((EntityReference)targetAluguel["smt_veiculo"], service);

                            // Atualiza o campo smt_qtdehoras

                            // Atualiza o campo smt_valortotal
                            Money valorTotal = AtualizarValorTotalCreate(targetAluguel, valordiaria, service, diferencaHoras);

                            targetAluguel["smt_valortotal"] = valorTotal;
                            targetAluguel["smt_qtdehoras"] = diferencaHoras;



                        }
                    }
                }
            }
            else if (context.MessageName.ToLower() == "update" && context.PrimaryEntityName == "smt_aluguel")
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity targetAluguel = (Entity)context.InputParameters["Target"];

                    Entity retorno = service.Retrieve("smt_aluguel", targetAluguel.Id, new ColumnSet(true));

                    DateTime aluguelDe = (DateTime)retorno["smt_aluguelde"];
                    DateTime aluguelAte = (DateTime)retorno["smt_aluguelate"];

                    // Calcula a diferença em horas
                    int diferencaHoras = (int)(aluguelAte - aluguelDe).TotalHours;

                    // Obtém o valor diário do veículo
                    Money valordiaria = ObterValorDiarioVeiculo((EntityReference)retorno["smt_veiculo"], service);

                    Entity aluguelAtualizado = new Entity(retorno.LogicalName, retorno.Id);

                    // Atualiza o campo smt_qtdehoras
                    aluguelAtualizado["smt_qtdehoras"] = diferencaHoras;

                    // Atualiza o campo smt_valortotal
                    AtualizarValorTotalUpgrade(aluguelAtualizado, valordiaria, service);
                }
            }
        }

        private void AtualizarValorTotalUpgrade(Entity aluguel, Money valordiaria, IOrganizationService service)
        {
            if (aluguel.Attributes.Contains("smt_qtdehoras"))
            {
                int qtdeHoras = (int)aluguel["smt_qtdehoras"];

                // Atualiza o campo smt_valortotal
                decimal valorDiarioDecimal = valordiaria.Value;

                // Realiza a operação de divisão por 24
                decimal resultadoDecimal = qtdeHoras * (valorDiarioDecimal / 24);

                // Cria um novo objeto Money com o resultado
                Money resultadoMoney = new Money(resultadoDecimal);

                // Atualiza o campo smt_valortotal
                aluguel["smt_valortotal"] = resultadoMoney;

                service.Update(aluguel);
            }
        }

        private Money AtualizarValorTotalCreate(Entity aluguel, Money valordiaria, IOrganizationService service, int qtdeHoras)
        {          

                // Atualiza o campo smt_valortotal
                decimal valorDiarioDecimal = valordiaria.Value;

                // Realiza a operação de divisão por 24
                decimal resultadoDecimal = Math.Round((decimal)qtdeHoras * (valorDiarioDecimal / 24), 2);

                // Cria um novo objeto Money com o resultado
                Money resultadoMoney = new Money(resultadoDecimal);

                // Atualiza o campo smt_valortotal
                return resultadoMoney;

        }

        private Money ObterValorDiarioVeiculo(EntityReference veiculoRef, IOrganizationService service)
        {
            ColumnSet columns = new ColumnSet("smt_valordiario");
            Entity veiculo = service.Retrieve(veiculoRef.LogicalName, veiculoRef.Id, columns);

            return (Money)veiculo["smt_valordiario"];
        }
    }
}