Feature: ClaimTearOff
	As a BROWSER
	I want to be able to claim a tear off
	So that my claim will be recorded

Scenario: Claim A Tear Off
Given I have navigated to the public view page for a FLIER
When I claim a tear off for that FLIER 
Then I will be recorded as having claimed the flier once
And the number of claims against the FLIER will be incremented

Scenario: Cant Claim Two Tear Offs  Flier 
Given I have navigated to the public view page for a FLIER
And I have already claimed a tear off for that FLIER
When I claim a tear off for that FLIER 
Then I will be recorded as having claimed the flier once
And the FLIER tear off claims will remain the same

Scenario: View Flier Claims
Given I have navigated to the public view page for a FLIER 
And Someone has claimed a tear off for a FLIER
Then I should see the claimed tear offs for the FLIER

@TearOffNotification
Scenario: Tear Off Claim Publishes Tear Off Notification
Given I have navigated to the public view page for a FLIER
When I claim a tear off for that FLIER 
Then A Notification for that Tear Off should be published