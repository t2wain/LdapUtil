## Query data from Active Directory (AD)

The file LdapConfig.json is used to override the default LdapConfig object which contains all the parameters to perform the queries and retrieving certain user's properties. The values in the file are the current default.

You can use the library to retrieve information about the users or about the AD groups' members. For user, you have the option to retrieve the user's reporting manager heirarchy recursively up to the specified level. For AD group, the library will retrieve all the members, including members of nested AD group recursively.