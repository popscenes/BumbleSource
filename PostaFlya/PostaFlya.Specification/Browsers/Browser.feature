Feature: Login
	As a Participant
	I want to be able to log onto the site 
	so that I am Authenticated


@mytag
Scenario: Identity Provider Authentication Request
	Given i have navigated to the log on page 
	When i provide Select an Identity Provider 
	Then i will be Redirected to the Identity Providers Login Page

Scenario: Identity Provider Authentication Response
	Given i have recieve a resonse from an Identity Provider 
	Then My credentials will be used to log me in

Scenario: Account Create
	Given i am not yet a BROWSER with PARTICIPANT ROLE  
	When i provide correct CREDENTIALS 
	Then a BROWSER with PARTICIPANT ROLE will be created for me

Scenario: Switch to existing BROWSER on Login
	Given i am an existing BROWSER with PARTICIPANT ROLE
	And i am currently operating in a BROWSER with TEMPORARY ROLE 
	When i provide correct CREDENTIALS
	Then my registered BROWSER will be loaded as the ACTIVE BROWSER

Scenario: update personal details
	Given I am a BROWSER in PARTICIPANT ROLE
	When I update my profile details with the following data:
	  | Name | FirstName | MiddleNames | Surname  | Email          | Address                      | Avatar                               | WebSite         |
	  | User | FirstName | M           | LastName | user@email.com | -37.769:144.979 | 8F68AE77-0F61-4BFD-92AC-BFCA1CC5B9E2 | http://test.com |
	Then the profile details will be stored against my browser

Scenario: Browser Verifies Identity
	Given I am a PARTICIPANT without IdentityVerified ROLE
	When I verify my physical identity 
	Then I will have IdentityVerified ROLE

Scenario: Profile View
	Given There is an existing BROWSER with PARTICIPANT ROLE  
	When i navigate to the public profile view the existing BROWSER 
	Then i will see the existing BROWSERS fliers and tear off claims