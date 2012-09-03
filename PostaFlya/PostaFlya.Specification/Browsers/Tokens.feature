Feature: Token managment
	In order to interact with external websites
	As a user
	I want to be able to manage and rettrieve access tokens for the websites

@FlierImport
Scenario: Access Token Show external Token Status
	Given I am a BROWSER in PARTICIPANT ROLE  
	When I go to the TOKEN MANAGMENT SCREEN
	Then Then i will be shown a list of my external TOKENS and thier current status

Scenario: Access Token Request Token for a Source
	Given I am a BROWSER in PARTICIPANT ROLE  
	When I request an access token for a source
	Then Then i will be redirected to obtain a valid token


Scenario: Access Token Obtain for a Source
	Given I am a BROWSER in PARTICIPANT ROLE  
	When I obtain a valid token from the source
	Then Then i will be redircted back to the redirect page specified
	And I will have a valid access token for the source

