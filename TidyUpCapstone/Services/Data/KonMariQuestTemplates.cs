using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Services.Data
{
    public static class KonMariQuestTemplates
    {
        // Map KonMari categories to your existing ItemCategory system
        public static readonly Dictionary<string, List<string>> KonMariCategoryMapping = new()
        {
            { "clothing", new List<string> { "Clothing", "Shoes", "Accessories", "Bags" } },
            { "books", new List<string> { "Books", "Magazines", "Educational Materials" } },
            { "papers", new List<string> { "Documents", "Papers", "Office Supplies" } },
            { "kitchen", new List<string> { "Kitchen", "Appliances", "Cookware", "Dining" } },
            { "bathroom", new List<string> { "Health & Beauty", "Personal Care", "Bathroom Items" } },
            { "garage", new List<string> { "Tools", "Sports Equipment", "Automotive", "Garden" } },
            { "miscellaneous", new List<string> { "Electronics", "Home Decor", "Toys", "Other" } }
        };

        // Daily Quest Templates (3 per category = 21 total)
        public static List<QuestTemplate> GetDailyQuestTemplates()
        {
            var templates = new List<QuestTemplate>();

            // CLOTHING Category - Daily Quests
            templates.AddRange(new[]
            {
                new QuestTemplate
                {
                    QuestTitle = "Closet Clarity Morning",
                    QuestDescription = "Start your day by decluttering clothing items that no longer spark joy",
                    QuestObjective = "List 3 clothing items for decluttering",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 3,
                    TokenReward = 8.00m,
                    XpReward = 15,
                    Category = "clothing",
                    ValidationType = "auto_item_listing",
                    ValidationCriteria = "category:clothing,shoes,accessories,bags"
                },
                new QuestTemplate
                {
                    QuestTitle = "Wardrobe Joy Check",
                    QuestDescription = "Share your decluttering progress with the community",
                    QuestObjective = "Post about clothing decluttering in community",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 1,
                    TokenReward = 6.00m,
                    XpReward = 12,
                    Category = "clothing",
                    ValidationType = "auto_community_post",
                    ValidationCriteria = "post_type:tip,content_keywords:clothing,wardrobe,closet"
                },
                new QuestTemplate
                {
                    QuestTitle = "Style Support Helper",
                    QuestDescription = "Help others with their clothing decluttering journey",
                    QuestObjective = "Comment helpfully on 2 clothing-related posts",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 2,
                    TokenReward = 7.00m,
                    XpReward = 10,
                    Category = "clothing",
                    ValidationType = "auto_community_engagement",
                    ValidationCriteria = "action:comment,min_length:20,context_keywords:clothing,style"
                }
            });

            // BOOKS Category - Daily Quests
            templates.AddRange(new[]
            {
                new QuestTemplate
                {
                    QuestTitle = "Literary Liberation",
                    QuestDescription = "Free up space by finding books ready for new homes",
                    QuestObjective = "List 2 books for decluttering",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 2,
                    TokenReward = 5.00m,
                    XpReward = 10,
                    Category = "books",
                    ValidationType = "auto_item_listing",
                    ValidationCriteria = "category:books,magazines,educational"
                },
                new QuestTemplate
                {
                    QuestTitle = "Reading Reflection Share",
                    QuestDescription = "Share wisdom about letting go of books that served their purpose",
                    QuestObjective = "Create a thoughtful post about book decluttering",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 1,
                    TokenReward = 8.00m,
                    XpReward = 15,
                    Category = "books",
                    ValidationType = "auto_community_post",
                    ValidationCriteria = "post_type:tip,content_keywords:books,reading,knowledge,min_length:50"
                },
                new QuestTemplate
                {
                    QuestTitle = "Knowledge Encourager",
                    QuestDescription = "Support others in their book decluttering decisions",
                    QuestObjective = "Leave encouraging comments on book-related posts",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 3,
                    TokenReward = 6.00m,
                    XpReward = 12,
                    Category = "books",
                    ValidationType = "auto_community_engagement",
                    ValidationCriteria = "action:comment,min_length:15,context_keywords:books,reading"
                }
            });

            // PAPERS Category - Daily Quests
            templates.AddRange(new[]
            {
                new QuestTemplate
                {
                    QuestTitle = "Document Declutter",
                    QuestDescription = "Clear unnecessary papers to create mental clarity",
                    QuestObjective = "List 5 paper/document items for disposal",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 5,
                    TokenReward = 7.00m,
                    XpReward = 12,
                    Category = "papers",
                    ValidationType = "auto_item_listing",
                    ValidationCriteria = "category:documents,papers,office"
                },
                new QuestTemplate
                {
                    QuestTitle = "Digital Organization Tip",
                    QuestDescription = "Share your digital organization strategies",
                    QuestObjective = "Post tips about managing digital documents",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 1,
                    TokenReward = 9.00m,
                    XpReward = 18,
                    Category = "papers",
                    ValidationType = "auto_community_post",
                    ValidationCriteria = "post_type:tip,content_keywords:digital,documents,organization,paperless"
                },
                new QuestTemplate
                {
                    QuestTitle = "Organization Support",
                    QuestDescription = "Help others with their document organization",
                    QuestObjective = "React positively to 3 organization-focused posts",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 3,
                    TokenReward = 5.00m,
                    XpReward = 8,
                    Category = "papers",
                    ValidationType = "auto_community_engagement",
                    ValidationCriteria = "action:reaction,reaction_type:helpful,inspiring,context_keywords:organize,documents"
                }
            });

            // KITCHEN Category - Daily Quests
            templates.AddRange(new[]
            {
                new QuestTemplate
                {
                    QuestTitle = "Kitchen Counter Clarity",
                    QuestDescription = "Create a peaceful cooking environment by decluttering",
                    QuestObjective = "List 4 kitchen items you no longer use",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 4,
                    TokenReward = 8.00m,
                    XpReward = 15,
                    Category = "kitchen",
                    ValidationType = "auto_item_listing",
                    ValidationCriteria = "category:kitchen,appliances,cookware,dining"
                },
                new QuestTemplate
                {
                    QuestTitle = "Mindful Cooking Share",
                    QuestDescription = "Inspire others with your kitchen organization journey",
                    QuestObjective = "Share before/after or kitchen organization tips",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 1,
                    TokenReward = 10.00m,
                    XpReward = 20,
                    Category = "kitchen",
                    ValidationType = "auto_community_post",
                    ValidationCriteria = "post_type:achievement,tip,content_keywords:kitchen,cooking,organize"
                },
                new QuestTemplate
                {
                    QuestTitle = "Culinary Community Builder",
                    QuestDescription = "Support others in their kitchen decluttering",
                    QuestObjective = "Comment encouragingly on kitchen-related posts",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 2,
                    TokenReward = 6.00m,
                    XpReward = 12,
                    Category = "kitchen",
                    ValidationType = "auto_community_engagement",
                    ValidationCriteria = "action:comment,min_length:20,context_keywords:kitchen,cooking,appliances"
                }
            });

            // BATHROOM Category - Daily Quests
            templates.AddRange(new[]
            {
                new QuestTemplate
                {
                    QuestTitle = "Self-Care Space Refresh",
                    QuestDescription = "Transform your bathroom into a serene self-care space",
                    QuestObjective = "Declutter 3 bathroom/beauty products",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 3,
                    TokenReward = 7.00m,
                    XpReward = 14,
                    Category = "bathroom",
                    ValidationType = "auto_item_listing",
                    ValidationCriteria = "category:health,beauty,personal,bathroom"
                },
                new QuestTemplate
                {
                    QuestTitle = "Wellness Wisdom",
                    QuestDescription = "Share your self-care organization insights",
                    QuestObjective = "Post about bathroom/wellness organization",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 1,
                    TokenReward = 9.00m,
                    XpReward = 16,
                    Category = "bathroom",
                    ValidationType = "auto_community_post",
                    ValidationCriteria = "post_type:tip,content_keywords:bathroom,beauty,wellness,self-care"
                },
                new QuestTemplate
                {
                    QuestTitle = "Beauty Support Circle",
                    QuestDescription = "Encourage others in their wellness journey",
                    QuestObjective = "React positively to self-care related posts",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 4,
                    TokenReward = 5.00m,
                    XpReward = 10,
                    Category = "bathroom",
                    ValidationType = "auto_community_engagement",
                    ValidationCriteria = "action:reaction,reaction_type:love,helpful,context_keywords:beauty,wellness"
                }
            });

            // GARAGE Category - Daily Quests
            templates.AddRange(new[]
            {
                new QuestTemplate
                {
                    QuestTitle = "Garage Gateway Progress",
                    QuestDescription = "Create clear pathways and purposeful storage",
                    QuestObjective = "List 3 garage/tool items for decluttering",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 3,
                    TokenReward = 10.00m,
                    XpReward = 18,
                    Category = "garage",
                    ValidationType = "auto_item_listing",
                    ValidationCriteria = "category:tools,sports,automotive,garden"
                },
                new QuestTemplate
                {
                    QuestTitle = "Workshop Wisdom",
                    QuestDescription = "Share your garage organization strategies",
                    QuestObjective = "Create a post about garage/storage organization",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 12.00m,
                    XpReward = 22,
                    Category = "garage",
                    ValidationType = "auto_community_post",
                    ValidationCriteria = "post_type:tip,achievement,content_keywords:garage,tools,storage,organize"
                },
                new QuestTemplate
                {
                    QuestTitle = "Tool Time Helper",
                    QuestDescription = "Help others with their garage organization",
                    QuestObjective = "Comment helpfully on garage-related posts",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 2,
                    TokenReward = 8.00m,
                    XpReward = 14,
                    Category = "garage",
                    ValidationType = "auto_community_engagement",
                    ValidationCriteria = "action:comment,min_length:25,context_keywords:garage,tools,storage"
                }
            });

            // MISCELLANEOUS Category - Daily Quests
            templates.AddRange(new[]
            {
                new QuestTemplate
                {
                    QuestTitle = "Odds and Ends Evaluation",
                    QuestDescription = "Address miscellaneous items throughout your home",
                    QuestObjective = "List 5 miscellaneous items for decluttering",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 5,
                    TokenReward = 7.00m,
                    XpReward = 12,
                    Category = "miscellaneous",
                    ValidationType = "auto_item_listing",
                    ValidationCriteria = "category:electronics,decor,toys,other"
                },
                new QuestTemplate
                {
                    QuestTitle = "Decluttering Inspiration",
                    QuestDescription = "Share your creative decluttering solutions",
                    QuestObjective = "Post about unique decluttering discoveries",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 1,
                    TokenReward = 9.00m,
                    XpReward = 16,
                    Category = "miscellaneous",
                    ValidationType = "auto_community_post",
                    ValidationCriteria = "post_type:achievement,tip,content_keywords:declutter,organize,creative,solution"
                },
                new QuestTemplate
                {
                    QuestTitle = "Community Cheerleader",
                    QuestDescription = "Spread positivity in the decluttering community",
                    QuestObjective = "React positively to 5 community posts",
                    QuestType = QuestType.Daily,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 5,
                    TokenReward = 6.00m,
                    XpReward = 10,
                    Category = "miscellaneous",
                    ValidationType = "auto_community_engagement",
                    ValidationCriteria = "action:reaction,reaction_type:like,love,inspiring"
                }
            });

            return templates;
        }

        // Weekly Quest Templates (2 per category = 14 total)
        // Weekly Quest Templates (2 per category = 14 total)
        public static List<QuestTemplate> GetWeeklyQuestTemplates()
        {
            var templates = new List<QuestTemplate>();

            // CLOTHING Category - Weekly Quests
            templates.AddRange(new[]
            {
                new QuestTemplate
                {
                    QuestTitle = "Wardrobe Transformation Week",
                    QuestDescription = "Complete a comprehensive wardrobe decluttering session following KonMari principles",
                    QuestObjective = "Declutter entire clothing category mindfully",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 25,
                    TokenReward = 35.00m,
                    XpReward = 75,
                    Category = "clothing",
                    ValidationType = "auto_category_completion",
                    ValidationCriteria = "items:25,category:clothing,gratitude_actions:5"
                },
                new QuestTemplate
                {
                    QuestTitle = "Joy-Sparking Style Curator",
                    QuestDescription = "Curate a wardrobe that truly sparks joy in your daily life",
                    QuestObjective = "Complete joy evaluation for clothing items",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 45.00m,
                    XpReward = 90,
                    Category = "clothing",
                    ValidationType = "auto_joy_evaluation",
                    ValidationCriteria = "joy_check:complete,items:20,community_share:1"
                }
            });

                    // BOOKS Category - Weekly Quests
                    templates.AddRange(new[]
                    {
                new QuestTemplate
                {
                    QuestTitle = "Literary Collection Renewal",
                    QuestDescription = "Transform your book collection into a curated library of knowledge",
                    QuestObjective = "Complete comprehensive book category declutter",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 20,
                    TokenReward = 30.00m,
                    XpReward = 65,
                    Category = "books",
                    ValidationType = "auto_category_completion",
                    ValidationCriteria = "items:20,category:books,wisdom_sharing:2"
                },
                new QuestTemplate
                {
                    QuestTitle = "Knowledge Keeper's Journey",
                    QuestDescription = "Mindfully curate books that align with your current learning path",
                    QuestObjective = "Practice mindful book curation with gratitude",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 40.00m,
                    XpReward = 80,
                    Category = "books",
                    ValidationType = "auto_mindful_curation",
                    ValidationCriteria = "gratitude_practice:15,reflection_posts:1,items:15"
                }
            });

                    // PAPERS Category - Weekly Quests
                    templates.AddRange(new[]
                    {
                new QuestTemplate
                {
                    QuestTitle = "Digital Zen Master",
                    QuestDescription = "Create clarity by organizing both physical and digital documents",
                    QuestObjective = "Complete comprehensive paper organization",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 30,
                    TokenReward = 32.00m,
                    XpReward = 70,
                    Category = "papers",
                    ValidationType = "auto_organization_mastery",
                    ValidationCriteria = "items:30,category:papers,digital_tips:1"
                },
                new QuestTemplate
                {
                    QuestTitle = "Paperless Life Architect",
                    QuestDescription = "Design a sustainable system for managing life's paperwork",
                    QuestObjective = "Establish mindful document management system",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 42.00m,
                    XpReward = 85,
                    Category = "papers",
                    ValidationType = "auto_system_creation",
                    ValidationCriteria = "system_design:complete,items:25,efficiency_sharing:1"
                }
            });

                    // KITCHEN Category - Weekly Quests
                    templates.AddRange(new[]
                    {
                new QuestTemplate
                {
                    QuestTitle = "Culinary Space Harmony",
                    QuestDescription = "Transform your kitchen into a peaceful cooking sanctuary",
                    QuestObjective = "Complete kitchen category transformation",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 18,
                    TokenReward = 38.00m,
                    XpReward = 80,
                    Category = "kitchen",
                    ValidationType = "auto_space_transformation",
                    ValidationCriteria = "items:18,category:kitchen,cooking_inspiration:1"
                },
                new QuestTemplate
                    {
                        QuestTitle = "Mindful Cooking Curator",
                        QuestDescription = "Curate kitchen tools that support mindful meal preparation",
                        QuestObjective = "Practice intentional kitchen organization",
                        QuestType = QuestType.Weekly,
                        Difficulty = QuestDifficulty.Hard,
                        TargetValue = 1,
                        TokenReward = 44.00m,
                        XpReward = 88,
                        Category = "kitchen",
                        ValidationType = "auto_mindful_cooking",
                        ValidationCriteria = "intention_setting:complete,items:15,meal_mindfulness:3"
                    }
                });

                    // BATHROOM Category - Weekly Quests
                    templates.AddRange(new[]
                    {
                new QuestTemplate
                {
                    QuestTitle = "Self-Care Sanctuary Creation",
                    QuestDescription = "Design a bathroom space that supports daily self-care rituals",
                    QuestObjective = "Complete bathroom category mindful declutter",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 15,
                    TokenReward = 28.00m,
                    XpReward = 60,
                    Category = "bathroom",
                    ValidationType = "auto_sanctuary_creation",
                    ValidationCriteria = "items:15,category:bathroom,self_care_routine:1"
                },
                new QuestTemplate
                {
                    QuestTitle = "Wellness Ritual Designer",
                    QuestDescription = "Curate beauty and wellness items that truly serve your well-being",
                    QuestObjective = "Establish mindful beauty and wellness practices",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 36.00m,
                    XpReward = 75,
                    Category = "bathroom",
                    ValidationType = "auto_wellness_design",
                    ValidationCriteria = "wellness_audit:complete,items:12,routine_sharing:1"
                }
            });

                    // GARAGE Category - Weekly Quests
                    templates.AddRange(new[]
                    {
                new QuestTemplate
                {
                    QuestTitle = "Utility Space Liberation",
                    QuestDescription = "Transform your garage into an organized, functional space",
                    QuestObjective = "Complete garage category systematic organization",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 20,
                    TokenReward = 50.00m,
                    XpReward = 100,
                    Category = "garage",
                    ValidationType = "auto_space_liberation",
                    ValidationCriteria = "items:20,category:garage,organization_system:1"
                },
                new QuestTemplate
                {
                    QuestTitle = "Tool Mastery Journey",
                    QuestDescription = "Mindfully curate tools and equipment that truly serve your projects",
                    QuestObjective = "Practice intentional tool and equipment curation",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 55.00m,
                    XpReward = 110,
                    Category = "garage",
                    ValidationType = "auto_tool_mastery",
                    ValidationCriteria = "purpose_evaluation:complete,items:15,project_planning:1"
                }
            });

                    // MISCELLANEOUS Category - Weekly Quests
                    templates.AddRange(new[]
                    {
                new QuestTemplate
                {
                    QuestTitle = "Life's Finishing Touches",
                    QuestDescription = "Address the final category with patience and mindfulness",
                    QuestObjective = "Complete miscellaneous category with care",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 25,
                    TokenReward = 40.00m,
                    XpReward = 85,
                    Category = "miscellaneous",
                    ValidationType = "auto_category_completion",
                    ValidationCriteria = "items:25,category:miscellaneous,patience_practice:1"
                },
                new QuestTemplate
                {
                    QuestTitle = "Life Harmony Curator",
                    QuestDescription = "Complete your decluttering journey with intention and gratitude",
                    QuestObjective = "Practice advanced mindful curation techniques",
                    QuestType = QuestType.Weekly,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 48.00m,
                    XpReward = 95,
                    Category = "miscellaneous",
                    ValidationType = "auto_harmony_curation",
                    ValidationCriteria = "advanced_techniques:complete,gratitude_ceremony:1,reflection:deep"
                }
            });

            return templates;
        }

        // Special Quest Templates (1 per category = 7 total)
        public static List<QuestTemplate> GetSpecialQuestTemplates()
        {
            var templates = new List<QuestTemplate>();
            templates.AddRange(new[]
            {
            // KonMari Category Mastery Quests
                new QuestTemplate
                {
                    QuestTitle = "Clothing Master: Joy-Sparked Wardrobe",
                    QuestDescription = "Achieve the ultimate KonMari clothing transformation",
                    QuestObjective = "Complete mastery-level wardrobe curation",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 100.00m,
                    XpReward = 200,
                    Category = "clothing",
                    ValidationType = "auto_mastery_achievement",
                    ValidationCriteria = "items:30,category:clothing,posts:3,community_score:50,time_span:30_days"
                },
                new QuestTemplate
                {
                    QuestTitle = "Literary Legacy: Wisdom Curator",
                    QuestDescription = "Become a master of mindful book curation",
                    QuestObjective = "Achieve book collection mastery",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 85.00m,
                    XpReward = 175,
                    Category = "books",
                    ValidationType = "auto_mastery_achievement",
                    ValidationCriteria = "items:25,category:books,posts:2,community_score:40,wisdom_sharing:high"
                },
                new QuestTemplate
                {
                    QuestTitle = "Kitchen Zen: Culinary Space Master",
                    QuestDescription = "Transform your kitchen into a joy-sparking culinary haven",
                    QuestObjective = "Achieve kitchen organization mastery",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 1,
                    TokenReward = 75.00m,
                    XpReward = 150,
                    Category = "kitchen",
                    ValidationType = "auto_mastery_achievement",
                    ValidationCriteria = "items:20,category:kitchen,functionality_score:high,posts:2"
                },
                new QuestTemplate
                {
                    QuestTitle = "Digital Minimalist: Tech Life Balance",
                    QuestDescription = "Master the art of mindful digital possession curation",
                    QuestObjective = "Achieve digital minimalism mastery",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 1,
                    TokenReward = 80.00m,
                    XpReward = 160,
                    Category = "electronics",
                    ValidationType = "auto_mastery_achievement",
                    ValidationCriteria = "items:15,category:electronics,mindfulness_posts:1,community_engagement:medium"
                },

                // Community Leadership Quests
                new QuestTemplate
                {
                    QuestTitle = "Wisdom Keeper: Community Sage",
                    QuestDescription = "Become a source of inspiration and guidance for fellow declutterers",
                    QuestObjective = "Achieve community leadership status",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 150.00m,
                    XpReward = 300,
                    Category = "community",
                    ValidationType = "community_contribution",
                    ValidationCriteria = "helpful_posts:10,positive_reactions:100,mentorship_actions:20,consistency:high"
                },
                new QuestTemplate
                {
                    QuestTitle = "Joy Ambassador: Happiness Spreader",
                    QuestDescription = "Spread the KonMari philosophy of joy through community engagement",
                    QuestObjective = "Inspire others through joyful sharing",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 1,
                    TokenReward = 90.00m,
                    XpReward = 180,
                    Category = "community",
                    ValidationType = "joy_spreading",
                    ValidationCriteria = "inspiring_posts:5,joy_mentions:25,positive_feedback:50,streak_maintenance:14_days"
                },

                // Mindfulness & Gratitude Quests
                new QuestTemplate
                {
                    QuestTitle = "Gratitude Guardian: Thankfulness Master",
                    QuestDescription = "Master the art of gratitude in the decluttering process",
                    QuestObjective = "Complete the gratitude transformation journey",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 50,
                    TokenReward = 60.00m,
                    XpReward = 120,
                    Category = "mindfulness",
                    ValidationType = "gratitude_practice",
                    ValidationCriteria = "gratitude_actions:50,mindful_posts:3,reflection_entries:10"
                },
                new QuestTemplate
                {
                    QuestTitle = "Mindful Curator: Present Moment Master",
                    QuestDescription = "Practice mindful decision-making in every decluttering choice",
                    QuestObjective = "Achieve mindful curation mastery",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 1,
                    TokenReward = 85.00m,
                    XpReward = 170,
                    Category = "mindfulness",
                    ValidationType = "mindfulness_practice",
                    ValidationCriteria = "mindful_decisions:100,reflection_quality:high,consistency:21_days"
                },

                // Seasonal & Event Quests
                new QuestTemplate
                {
                    QuestTitle = "New Year, New Space: Fresh Start Champion",
                    QuestDescription = "Begin the year with a transformative decluttering journey",
                    QuestObjective = "Complete New Year transformation",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 100,
                    TokenReward = 120.00m,
                    XpReward = 240,
                    Category = "seasonal",
                    ValidationType = "seasonal_challenge",
                    ValidationCriteria = "items_listed:100,categories:3,community_sharing:high,timeline:january"
                },
                new QuestTemplate
                {
                    QuestTitle = "Spring Renewal: Rebirth Specialist",
                    QuestDescription = "Embrace spring's energy with comprehensive space renewal",
                    QuestObjective = "Complete spring renewal transformation",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 75,
                    TokenReward = 95.00m,
                    XpReward = 190,
                    Category = "seasonal",
                    ValidationType = "seasonal_challenge",
                    ValidationCriteria = "items_listed:75,deep_clean_areas:5,nature_inspired_posts:2"
                },

                // Challenge & Achievement Quests
                new QuestTemplate
                {
                    QuestTitle = "Minimalist Warrior: Ultimate Simplicity",
                    QuestDescription = "Achieve the highest level of mindful minimalism",
                    QuestObjective = "Complete minimalist mastery challenge",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 200.00m,
                    XpReward = 400,
                    Category = "achievement",
                    ValidationType = "ultimate_challenge",
                    ValidationCriteria = "items_listed:250,categories_completed:5,community_leadership:high,consistency:60_days"
                },
                new QuestTemplate
                {
                    QuestTitle = "Transformation Catalyst: Change Agent",
                    QuestDescription = "Become a catalyst for positive change in your community",
                    QuestObjective = "Complete transformation catalyst journey",
                    QuestType = QuestType.Special,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 1,
                    TokenReward = 175.00m,
                    XpReward = 350,
                    Category = "achievement",
                    ValidationType = "catalyst_impact",
                    ValidationCriteria = "personal_transformation:complete,others_inspired:10,community_impact:high,sustained_change:45_days"
                }
            });

            return templates;
        }
    }

    // Quest Template class for easier management
    public class QuestTemplate
    {
        public string QuestTitle { get; set; } = string.Empty;
        public string QuestDescription { get; set; } = string.Empty;
        public string QuestObjective { get; set; } = string.Empty;
        public QuestType QuestType { get; set; }
        public QuestDifficulty Difficulty { get; set; }
        public int TargetValue { get; set; }
        public decimal TokenReward { get; set; }
        public int XpReward { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ValidationType { get; set; } = string.Empty;
        public string ValidationCriteria { get; set; } = string.Empty;
    }
}