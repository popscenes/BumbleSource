Feature: ClaimTearOff
	As a BROWSER
	I want to be able to claim a tear off
	So that my claim will be recorded

Scenario: Claim An Initial Tear Off
Given I have navigated to the public view page for a FLIER With TEAR OFF
And The Flier Creator Has Sufficirnt Credit
When I claim a tear off for that FLIER 
Then I will be recorded as having claimed the flier once
And the number of claims against the FLIER will be incremented
And 2.00 will be deducted from the Flier Creators Account

Scenario: Claim An Initial Tear Off And Send Contact Details
Given I have navigated to the public view page for a FLIER With TEAR OFF And USER CONTACT
And The Flier Creator Has Sufficirnt Credit
When I claim a tear off for that FLIER and send my contact details
Then I will be recorded as having claimed the flier once
And the number of claims against the FLIER will be incremented
And the Claim will be ecorded as having My Contact Details
And 5.00 will be deducted from the Flier Creators Account

Scenario: Claim A Tear Off When One Has Been Claimed and Send Contact Details
Given I have navigated to the public view page for a FLIER With TEAR OFF And USER CONTACT
And The Flier Creator Has Sufficirnt Credit
And I have already claimed a tear off for that FLIER
When I claim a tear off for that FLIER and send my contact details 
Then I will be recorded as having claimed the flier once
And the number of claims against the FLIER will not be incremented
And the Claim will be ecorded as having My Contact Details
And 7.00 will be deducted from the Flier Creators Account

Scenario: Claim A Tear Off When One Has Been Claimed
Given I have navigated to the public view page for a FLIER With TEAR OFF
And The Flier Creator Has Sufficirnt Credit
And I have already claimed a tear off for that FLIER
When Another Browser claims a tear off for that FLIER 
Then I will be recorded as having claimed the flier once
And the number of claims against the FLIER will be incremented
And 2.00 will be deducted from the Flier Creators Account


Scenario: Cant Claim Two Tear Offs  Flier 
Given I have navigated to the public view page for a FLIER With TEAR OFF
And I have already claimed a tear off for that FLIER
When I claim a tear off for that FLIER 
Then I will be recorded as having claimed the flier once
And the FLIER tear off claims will remain the same

Scenario: View Flier Claims
Given I have navigated to the public view page for a FLIER With TEAR OFF 
And Someone has claimed a tear off for a FLIER
Then I should see the claimed tear offs for the FLIER

@TearOffNotification
Scenario: Tear Off Claim Publishes Tear Off Notification
Given I have navigated to the public view page for a FLIER
When I claim a tear off for that FLIER 
Then A Notification for that Tear Off should be published

