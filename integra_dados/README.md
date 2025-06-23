
# 🌦️ Forecast Controller

Este microsserviço fornece uma API para gerenciamento de previsões meteorológicas associadas a locais geográficos. Através dele, é possível cadastrar, consultar, editar e excluir registros de previsão, além de obter dados atualizados de previsão do tempo.

## 📦 Estrutura

- **Framework:** ASPNET CORE
- **Anotações:** OpenAPI/Swagger para documentação de endpoints
- **Camadas:**
    - Controller
    - Service (`WeatherService`)
    - Model (`ForecastRegistry`, `Location`, `ResponseClient`)

## 🚀 Endpoints
  # Route /forecast/
| Método     | Endpoint           | Descrição                                              | Status de Resposta                                           |
|------------|--------------------|--------------------------------------------------------| ------------------------------------------------------------ |
| **GET**    | `/location`        | Busca previsões para uma localização (lat, lng)        | 200 ✅ Sucesso<br>500 ❌ Erro interno                          |
| **POST**   | `/create`          | Cria um novo registro de previsão                      | 200 ✅ Criado<br>409 ⚠️ Já existe<br>500 ❌ Erro interno       |
| **PUT**    | `/edit`            | Edita um registro existente de previsão                | 200 ✅ Editado<br>500 ❌ Erro interno                          |
| **DELETE** | `/delete?id={id}`  | Deleta um registro pelo `id`                           | 200 ✅ Deletado                                               |
| **GET**    | `/registries/{name}` | Retorna todos os registros associados à variabel `name` | 200 ✅ Deletado                                               |

## 🔄 Envio periódico ao Broker

- Executado a cada 1 minuto
- Função: sendForecastInformationToBroker()
- Envia as informações atualizadas dos registros (UpdatedForecastRegistries) para o broker de mensagens.


## 🗺️ Modelos principais
### ForecastRegistry
- Representa um registro de previsão.
### Location
- Contém latitude e longitude.
### ResponseClient
- Retorno padrão da API, com mensagens e status HTTP.
## 📑 Observações

- Este controller é uma classe abstrata, devendo ser estendida para cada variável específica de previsão (ex.: temperatura, umidade, etc.).
- O método de envio ao broker é executado automaticamente, sem necessidade de chamada externa.


# 🛠️ Supervisory Controller

Este microsserviço fornece uma API para gerenciamento de registros de supervisão (Supervisory) associados a sistemas. Através dele, é possível cadastrar, consultar, editar e excluir registros, além de enviar informações periodicamente para um broker.

## 📦 Estrutura

- **Framework:** ASPNET CORE
- **Anotações:** OpenAPI/Swagger para documentação de endpoints
- **Camadas:**
    - Controller
    - Service (`SupervisoryService`)
    - Model (`SupervisoryRegistry`, `ResponseClient`)

## 🚀 Endpoints
| Método     | Endpoint               | Descrição                                                 | Status de Resposta                                           |
|------------|------------------------|-----------------------------------------------------------| ------------------------------------------------------------ |
| **GET**    | `/{id}`                | Busca previsões para uma localização (lat, lng)           | 200 ✅ Sucesso<br>500 ❌ Erro interno                          |
| **GET**    | `/variable/{id}/all  ` | Lista todos os registros para a variável `id` configurada | 200 ✅ Sucesso<br>404 ⚠️ Não encontrado<br>500 ❌ Erro interno |
| **POST**   | `/create`              | Cria um novo registro de previsão                         | 200 ✅ Criado<br>409 ⚠️ Já existe<br>500 ❌ Erro interno       |
| **POST**   | `/edit`                | Edita um registro existente de previsão                   | 200 ✅ Editado<br>500 ❌ Erro interno                          |
| **DELETE** | `/delete?id={id}`      | Deleta um registro pelo `id`                              | 200 ✅ Deletado                                               |
| **GET**    | `/all`                 | Retorna todos os registros ativos                         | 200 ✅ Deletado                                               |

## 🔄 Envio periódico ao Broker

- Executado a cada 1 segundo
- Função: `sendSupervisoryInformationToBroker()`
- Envia as informações atualizadas dos registros (`UpdatedSupervisoryRegistries`) para o broker de mensagens.

## 🗺️ Modelos principais

### SupervisoryRegistry
- Representa um registro de supervisão.
### ResponseClient
- Retorno padrão da API, com mensagens e status HTTP.

## 📑 Observações
- Este controller não é abstrato, está pronto para uso direto.
- O método de envio ao broker é executado automaticamente, sem necessidade de chamada externa.