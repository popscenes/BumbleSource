Feature: ClaimTearOff
	As a BROWSER
	I want to be able to claim a tear off
	So that my claim will be recorded

Scenario: Claim An Initial Tear Off
Given I am a BROWSER in PARTICIPANT ROLE
And I have navigated to the public view page for a FLIER With TEAR OFF
When I claim a tear off for that FLIER 
Then I will be recorded as having claimed the flier once
And the number of claims against the FLIER will be incremented

#Scenario: Claim An Initial Tear Off And Send Contact Details
#Given I have navigated to the public view page for a FLIER With TEAR OFF And USER CONTACT
#And The Flier Creator Has 1000 Account Credits
#When I claim a tear off for that FLIER and send my contact details
#Then I will be recorded as having claimed the flier once
#And the number of claims against the FLIER will be incremented
#And the Claim will be recorded as having My Contact Details
#And 500 will be deducted from the Flier Creators Account

Scenario: Claim A Tear Off When One Has Been Claimed
Given I am a BROWSER in PARTICIPANT ROLE
And I have navigated to the public view page for a FLIER With TEAR OFF
And I have already claimed a tear off for that FLIER
When Another Browser claims a tear off for that FLIER 
Then the number of claims against the FLIER will be incremented

Scenario: Cant Claim Two Tear Offs  Flier 
Given I am a BROWSER in PARTICIPANT ROLE
And I have navigated to the public view page for a FLIER With TEAR OFF
And I have already claimed a tear off for that FLIER
When I claim a tear off for that FLIER 
Then I will be recorded as having claimed the flier once
And the FLIER tear off claims will remain the same

#Scenario: View Flier Claims
#Given I have navigated to the public view page for a FLIER With TEAR OFF 
#And Someone has claimed a tear off for a FLIER
#Then I should see the claimed tear offs for the FLIER

@TearOffNotification
Scenario: Tear Off Claim Publishes Tear Off Notification
Given I am a BROWSER in PARTICIPANT ROLE
And I have navigated to the public view page for a FLIER
When I claim a tear off for that FLIER 
Then A Notification for that Tear Off should be published

Scenario: Can See Contact Details on flier Once Tear Off Is Claimed
Given I am a BROWSER in PARTICIPANT ROLE
And I have navigated to the public view page for a FLIER With TEAR OFF
And I have already claimed a tear off for that FLIER
When I have navigated to the public view page for that FLIER
Then I should see the public details of that FLIER
And I should see the Contact Details associated with that FLIER

