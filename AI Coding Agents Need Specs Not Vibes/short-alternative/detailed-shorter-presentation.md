**AI coding agents are powerful, but they are not mind readers.**
Vague prompting works for quick prototypes, but reliable delivery comes from treating the **spec as the source of truth**, then using a structured workflow to turn intent into code. 

## Recommended presentation structure for 20 minutes

### 1. Opening: the problem everyone recognizes

**Time:** 1.5 min

**Goal:** Get immediate audience buy-in.

**Subsections**

* **Hook:** “AI can write code fast. So why do we still get wrong implementations?”
* **Thesis slide:** “AI Coding Agents Need Specs, Not Vibes”
* **Framing sentence:** Vibe coding is attractive because output is fast, but the problem is not speed; the problem is misalignment.

**What to say**
Open with a familiar situation: “Add feature X,” the agent produces a lot of code quickly, it looks plausible, but misses intent, architecture, edge cases, or constraints.

**Why this section matters**
It aligns with the recurring message across your sources: code generation is cheap now, but good code still has a cost, and new habits are needed around coding agents.

---

### 2. Small contrast section: what vibe coding looks like, and why it breaks

**Time:** 3 min

**Goal:** Use vibe coding as the setup, not the main topic.

**Subsections**

* **What vibe coding is**
* **Why it feels great**
* **Where it fails**

**What to show**
A very short example:

* Prompt: “Add photo sharing to my app” or something similar in your own domain
* Agent output: plausible but full of assumptions
* Misses: scope, non-goals, architecture fit, constraints, verification

**Core message**
Vibe coding is acceptable for exploration and quick prototypes. It becomes dangerous when the task is real, shared, stateful, or long-lived.

**What to emphasize**
Do not overdo the “vibes are bad” angle. The stronger point is:
**vibe coding is for exploration; spec-driven development is for reliable engineering.**

GitHub explicitly makes this distinction, saying vibe coding can be fine for quick prototypes but is less reliable for serious applications or existing codebases. Augment makes the same point by saying not every task needs a spec, and that spec overhead is justified when misalignment is expensive to reverse.

---

### 3. The core concept: what SDD actually changes

**Time:** 3 min

**Goal:** Move from criticism to the actual model.

**Subsections**

* **From code as truth to spec as truth**
* **Spec as contract**
* **Agent as pair programmer, not autopilot**

**What to say**
This is the conceptual center of the talk:

* Traditional reality: code becomes the de facto truth
* SDD flips that: the spec becomes the authoritative artifact
* Code is then generated, implemented, or verified against that spec

**Key line**
“Specs are not documentation after the fact. They are executable intent.”

**Why this section matters**
This is the strongest academic and practical framing in your sources. The arXiv paper defines SDD as treating specifications as the source of truth and code as a generated or verified secondary artifact. Augment describes the spec as a contract that defines what to build, what not to build, and how to verify it. GitHub similarly describes specs as living, executable artifacts and a shared source of truth.

---

### 4. What a good spec contains

**Time:** 4 min

**Goal:** Make SDD tangible.

**Subsections**

* **Outcome**
* **Scope / out of scope**
* **Constraints and assumptions**
* **Decisions already made**
* **Task breakdown**
* **Verification criteria**

**What to say**
This is where you stop being philosophical and become concrete.

A good slide here is:
**“A spec answers six questions”**

1. What will be true when the work is done?
2. What is in scope?
3. What is explicitly out of scope?
4. What constraints or assumptions must the agent respect?
5. What decisions are already fixed?
6. How will we verify success?

**Optional extra subsection**

* **Project constitution / team rules**

  * stack
  * architecture principles
  * security constraints
  * style / standards

That idea is reinforced by the DeepLearning.AI course framing around a **project constitution**, feature specs, and a **plan-implement-validate** loop.

**Best demo move**
Show:

* one vague prompt
* one structured spec snippet
* ask the audience which one gives the agent less room to guess

---

### 5. The workflow: how SDD works with coding agents

**Time:** 5 min

**Goal:** Give the audience an operational model they can use the next day.

**Subsections**

* **Specify** — define the what and why
* **Plan** — define architecture, stack, constraints
* **Tasks** — break into small reviewable chunks
* **Implement + Verify** — agent executes, human reviews

**What to say**
The most useful practical workflow from your sources is:

1. **Specify**
   The human defines the problem, outcomes, and boundaries.

2. **Plan**
   The human orients the agent with architecture, tech choices, compliance, performance, integration constraints.

3. **Tasks**
   Break the work into small units that can be reviewed and tested independently.

4. **Implement and verify**
   The agent writes the code, but the human remains the supervisor: review, challenge, refine, accept, or reject.

This lines up closely with GitHub’s spec / plan / tasks / implement flow, and with the DeepLearning.AI “plan, implement, validate” framing. GitHub also stresses that your role is not just to steer but to verify at each checkpoint.
**Best emphasis**
This is where you can say:

> “The agent provides speed and implementation capacity.
> The developer provides intent, constraints, and judgment.”

That is probably one of your strongest lines.

---

### 6. When SDD is worth it — and when it is not

**Time:** 2 min

**Goal:** Add nuance and credibility.

**Subsections**

* **Use SDD when**
* **Don’t use SDD when**

**Use SDD when**

* feature work touches architecture
* multiple sessions are involved
* team handoff matters
* you need testable acceptance criteria
* you are changing an existing system
* you are modernizing legacy code

**Do not use full SDD when**

* it is a tiny low-risk task
* the work is exploratory
* you can review the result in minutes
* the cost of being wrong is low

This nuance is important because the paper explicitly includes a decision framework for when SDD adds value, Augment says not every task needs a spec, and GitHub identifies three especially strong scenarios: greenfield, feature work in existing systems, and legacy modernization.

---

### 7. Closing: the shift in mindset

**Time:** 1.5 min

**Goal:** Leave the audience with a memorable takeaway.

**Subsections**

* **Old model**
* **New model**
* **Practical takeaway**

**Suggested close**

* Old model: “code is the source of truth”
* New model: “intent becomes the source of truth”
* Final takeaway: **If the task matters, don’t ask the agent for code first. Ask it to help you write the spec first.**

That closing is well supported by both the paper and GitHub’s framing that the field is moving from code-centric truth toward intent- and spec-centric truth.
