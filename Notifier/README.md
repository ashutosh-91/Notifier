 ## Notifier

![Screenshot 2026-11-16 at 6.31.14 PM.png](../../../../../Desktop/Screenshot%202026-11-16%20at%206.31.14%E2%80%AFPM.png)

 ## What can be done better?
 - Right now the requests can be duplicated that is Request 1 (Id : 100) Request 2 (Id : 100) and we still process it. This can be prevented using a persistent layer such as DB to store the Idempotent keys this would prevent duplicate LLM calls as well.
 - Instead of just rate limitting adding a retry mechanism for failures.
 - Add Logging at each level.
