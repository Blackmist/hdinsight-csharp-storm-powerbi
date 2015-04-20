This example demonstrates how you can send data to Microsoft Power BI Preview (powerbi.com,) from a Storm topology running on HDInsight. All the magic happens in the following code:

* **PowerBiBolt.cs**: Implements the Storm bolt, which sends data to Power BI

* **Data.cs**: Describes the data object/row that will be sent to Power BI

* **JSONBuilder.cs**: Contains methods used to create the JSON structures expected by Power BI

Otherwise, it's a pretty basic word count example.

Since there's currently (4/20/2015,) no official .NET client for Power BI, this code directly talks to the PowerBI REST API. The code that talks to Power BI was adapted from the [https://github.com/PowerBI/getting-started-for-dotnet](https://github.com/PowerBI/getting-started-for-dotnet) sample.

## Prerequisites

* Visual Studio

* An Azure subscription

* An Azure Active Directory account with access to PowerBI.com

* The HDInsight tools for Visual Studio

* A Storm on HDInsight cluster

## To use this example

1. Follow the steps in the [Power BI quickstart](https://msdn.microsoft.com/en-US/library/dn931989.aspx) to sign up for Power BI.

2. Follow the steps in [Register an app](https://msdn.microsoft.com/en-US/library/dn877542.aspx) to create an application registration. This will be used when accessing the Power BI REST API.

    > [AZURE.IMPORTANT] Save the **Client ID** for the application registration.

3. Clone/fork/download a zip of this sample.

4. Open the solution in Visual Studio.

5. Right-click on the **WordCount** project and select properties, then select the **Settings** tab. Fill in the following entries:

	* **PowerBiClientId**: The Client ID for the application registration you created earlier.

    * **PowerBiUsername**: An Azure Active Directory account that has access to Power BI.

    * **PowerBiPassword**: The password for the Azure Active Directory account.

6. Right-click on the **WordCount** project and select **Submit to Storm on HDInsight**. Select the Storm cluster name and deploy.

Once deployed, the topology will randomly emit sentences, count the occurrence of words, and emit the word and count to Power BI. If you visit http://powerbi.com and login as the user, you will see a new **Words** dataset, which can be used to create reports or a live dashboard. 