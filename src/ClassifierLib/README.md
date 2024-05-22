## Business Rule using Regular Expression Pattern

Business rule and rule engine are well established design concepts. This specific implementation of business rule and rule engine will use regular expression to determine matching rules. Regular expression pattern is the preferred choice since it is a standard technology that is well understood by users who author business rules.

## Rule Authoring

- A Rule has a collection of RuleMatch
- A RuleMatch has a collection of Include and Exclude regular expressions
- A Rule is matched if there is a match from the Include collection
- However, afterward, the match is negated if there is a match from the Exclude collection
- A match can be specified based on either one match or all matches from the collection (RuleMatch, Include, and/or Exclude)

The rule set is authored in an external XML file.

```xml
<Rules>
	<!-- one or more Rule -->
	<Rule>
		<Name>...</Name>
		<Description>...</Description>
		<!-- require one match or match all -->
		<IsMatchAll>false</IsMatchAll>
		<Matches>
			<!-- one or more RuleMatch -->
			<RuleMatch>
				<Name>...</Name>
				<!-- require one match or match all -->
				<IsMatchAllExclExpr>false</IsMatchAllExclExpr>
				<IsMatchAllInclExpr>false</IsMatchAllInclExpr>
				<Includes>
					<!-- one or more RegExpr -->
					<RegExpr>...</RegExpr>
				</Includes>
				<Excludes>
					<!-- one or more RegExpr -->
					<RegExpr>...</RegExpr>
				</Excludes>
			</RuleMatch>
		</Matches>
	</Rule>
</Rules>
```

## Specific Use Case

We have a need to determine which business units are users belong to based on users' home office locations and reporting hierarchy. Information about users is queried from Active Directory (AD). Each Rule is authored to match a particular business unit. User info is appended into a delimited key-value pair text as input data.

```csharp
$"UserName={user.UserID};Name={user.Name};Address={user.Address};City={user.Location};State={user.State};Country={user.Country};Department={user.Department};Organization={user.BusinessGroup};Division={user.Division};Company={user.Company};Managers=#{string.Join("#", user.Managers.Select(m => m.UserID).ToArray())}#;UserID={user.UserID};";
```